using GrowMate.Contracts.Requests.Payment;
using GrowMate.Contracts.Requests.Tree;
using GrowMate.Contracts.Requests.Adoption;
using GrowMate.Contracts.Responses.Payment;
using GrowMate.Contracts.Responses.Auth;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace GrowMate.Services.Payments
{
    public interface IPaymentService
    {
        Task<PageResult<PaymentResponse>> GetByOrderIdAsync(int orderId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<PaymentResponse>> GetByCustomerIdAsync(int customerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<PaymentResponse>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<PaymentResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PaymentResponse?> GetByIdAsync(int paymentId, CancellationToken ct = default);
        Task<PaymentDetailResponse?> GetDetailAsync(int paymentId, CancellationToken ct = default);
        Task<AuthResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdatePaymentAsync(int paymentId, UpdatePaymentRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdatePaymentStatusAsync(int paymentId, UpdatePaymentStatusRequest request, CancellationToken ct = default);
        Task<AuthResponse> DeletePaymentAsync(int paymentId, CancellationToken ct = default);
        Task<PaymentQrResponse> CreateSepayQrAsync(int orderId, int? expiresMinutes = 10, CancellationToken ct = default);
        Task<AuthResponse> ProcessSepayWebhookAsync(string authorizationHeader, string payloadJson, CancellationToken ct = default);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;

        public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<PageResult<PaymentResponse>> GetByOrderIdAsync(int orderId, int page, int pageSize, CancellationToken ct = default)
        {
            var payments = await _unitOfWork.Payments.GetByOrderIdAsync(orderId, page, pageSize, ct);
            return await MapPaymentsToResponseAsync(payments, ct);
        }

        public async Task<PaymentQrResponse> CreateSepayQrAsync(int orderId, int? expiresMinutes = 10, CancellationToken ct = default)
        {
            // Load order
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Không tìm thấy orderId: {orderId}");
            }

            // Reuse existing unexpired PENDING QR for this order (idempotent within TTL)
            var existing = await _unitOfWork.Payments.GetByOrderIdAsync(orderId, 1, 5, ct);
            var reuse = existing.Items
                .FirstOrDefault(p => string.Equals(p.Status, "PENDING", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals(p.PaymentGateway, "SEPAY", StringComparison.OrdinalIgnoreCase)
                                     && p.ExpiresAt.HasValue && p.ExpiresAt.Value > DateTime.Now);
            if (reuse != null)
            {
                return new PaymentQrResponse
                {
                    PaymentId = reuse.PaymentId,
                    OrderId = orderId,
                    GatewayOrderCode = reuse.GatewayOrderCode,
                    QrContent = reuse.QrContent,
                    QrImageUrl = reuse.QrImageUrl,
                    ExpiresAt = reuse.ExpiresAt,
                    TransactionReference = reuse.TransactionReference,
                    Status = reuse.Status
                };
            }

            // Build QR using VietQR style (no external call needed)
            var bankCode = _configuration["Sepay:BankCode"];
            var accountNumber = _configuration["Sepay:AccountNumber"];
            if (string.IsNullOrWhiteSpace(bankCode) || string.IsNullOrWhiteSpace(accountNumber))
            {
                throw new InvalidOperationException("Thiếu cấu hình Sepay:BankCode hoặc Sepay:AccountNumber");
            }

            // Fixed transfer amount for QR (VND)
            var amount = 5000m;
            var gatewayOrderCode = $"ORD{orderId}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var addInfo = $"GROWMATE-{gatewayOrderCode}";
            var expiresAt = DateTime.Now.AddMinutes(expiresMinutes ?? 10);

            // Sepay QR (theo tài liệu: https://qr.sepay.vn/img?acc=SO_TAI_KHOAN&bank=NGAN_HANG&amount=SO_TIEN&des=NOI_DUNG)
            var vndInt = (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero);
            var qrImageUrl = $"https://qr.sepay.vn/img?acc={Uri.EscapeDataString(accountNumber)}&bank={Uri.EscapeDataString(bankCode)}&amount={vndInt}&des={Uri.EscapeDataString(addInfo)}";

            // Save PENDING payment snapshot
            var payment = new Models.Payment
            {
                OrderId = orderId,
                Amount = amount,
                PaymentMethod = "SEPAY",
                Status = "PENDING",
                SourceType = "WEB",
                CreatedAt = DateTime.Now,
                PaymentGateway = "SEPAY",
                GatewayOrderCode = gatewayOrderCode,
                QrContent = addInfo,
                QrImageUrl = qrImageUrl,
                ExpiresAt = expiresAt
            };

            await _unitOfWork.Payments.AddAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new PaymentQrResponse
            {
                PaymentId = payment.PaymentId,
                OrderId = orderId,
                GatewayOrderCode = gatewayOrderCode,
                QrContent = addInfo,
                QrImageUrl = qrImageUrl,
                ExpiresAt = expiresAt,
                TransactionReference = null,
                Status = payment.Status
            };
        }

        public async Task<AuthResponse> ProcessSepayWebhookAsync(string authorizationHeader, string payloadJson, CancellationToken ct = default)
        {
            try
            {
                var expected = _configuration["Sepay:WebhookToken"];
                if (!string.IsNullOrEmpty(expected))
                {
                    var expectedHeader = $"Apikey {expected}";
                    if (!string.Equals(authorizationHeader, expectedHeader, StringComparison.Ordinal))
                    {
                        return new AuthResponse { Success = false, Message = "Chứng thực webhook không hợp lệ." };
                    }
                }

                using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
                var root = doc.RootElement;
                // best-effort parse
                var amount = root.TryGetProperty("amount", out var amountProp) ? amountProp.GetDecimal() : 0m;
                var description = root.TryGetProperty("description", out var descProp) ? descProp.GetString() : root.TryGetProperty("content", out var cProp) ? cProp.GetString() : string.Empty;
                var transactionRef = root.TryGetProperty("transaction_code", out var trProp) ? trProp.GetString() : root.TryGetProperty("transId", out var tr2) ? tr2.GetString() : null;

                // find by gateway_order_code from description/content
                // Accept formats: "GROWMATE-<code>", "GROWMATE <code>", or "GROWMATE<code>"
                string? gatewayOrderCode = null;
                if (!string.IsNullOrEmpty(description))
                {
                    var match = Regex.Match(description, @"GROWMATE[-:\s]?([A-Za-z0-9\-_]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        gatewayOrderCode = match.Groups[1].Value;
                    }
                }

                Models.Payment? payment = null;
                if (!string.IsNullOrEmpty(gatewayOrderCode))
                {
                    payment = await _unitOfWork.Payments.GetByGatewayOrderCodeAsync(gatewayOrderCode, ct);
                }
                if (payment == null && !string.IsNullOrEmpty(transactionRef))
                {
                    payment = await _unitOfWork.Payments.GetByTransactionReferenceAsync(transactionRef, ct);
                }
                if (payment == null)
                {
                    return new AuthResponse { Success = false, Message = "Không tìm thấy payment tương ứng." };
                }

                if (string.Equals(payment.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    return new AuthResponse { Success = true, Message = "Đã xử lý trước đó." };
                }

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    payment.Status = "SUCCESS";
                    payment.PaidAt = DateTime.Now;
                    if (!string.IsNullOrEmpty(transactionRef)) payment.TransactionReference = transactionRef;
                    payment.WebhookReceivedAt = DateTime.Now;
                    payment.GatewayRawPayload = payloadJson;

                    _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.SaveChangesAsync(innerCt);

                    // Load order with items if needed
                    var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId ?? 0);
                    if (order != null)
                    {
                        await CreateAdoptionsAndTreesFromOrderAsync(order, innerCt);
                    }
                }, ct);

                return new AuthResponse { Success = true, Message = "Ghi nhận thanh toán thành công." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessSepayWebhookAsync error");
                return new AuthResponse { Success = false, Message = "Xử lý webhook thất bại: " + ex.Message };
            }
        }

        public async Task<PageResult<PaymentResponse>> GetByCustomerIdAsync(int customerId, int page, int pageSize, CancellationToken ct = default)
        {
            var payments = await _unitOfWork.Payments.GetByCustomerIdAsync(customerId, page, pageSize, ct);
            return await MapPaymentsToResponseAsync(payments, ct);
        }

        public async Task<PageResult<PaymentResponse>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var payments = await _unitOfWork.Payments.GetByFarmerIdAsync(farmerId, page, pageSize, ct);
            return await MapPaymentsToResponseAsync(payments, ct);
        }

        public async Task<PageResult<PaymentResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var payments = await _unitOfWork.Payments.GetAllAsync(page, pageSize, ct);
            return await MapPaymentsToResponseAsync(payments, ct);
        }

        public async Task<PaymentResponse?> GetByIdAsync(int paymentId, CancellationToken ct = default)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, ct);
            if (payment == null) return null;

            return await MapPaymentToResponseAsync(payment, ct);
        }

        public async Task<PaymentDetailResponse?> GetDetailAsync(int paymentId, CancellationToken ct = default)
        {
            var payment = await _unitOfWork.Payments.GetByIdWithDetailsAsync(paymentId, ct);
            if (payment == null) return null;

            return await MapPaymentToDetailResponseAsync(payment, ct);
        }

        public async Task<AuthResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default)
        {
            try
            {
                // Validate order exists
                var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    return new AuthResponse { Success = false, Message = $"Không tìm thấy orderId: {request.OrderId}" };
                }

                // Check if transaction reference already exists
                if (await _unitOfWork.Payments.ExistsByTransactionReferenceAsync(request.TransactionReference, ct))
                {
                    return new AuthResponse { Success = false, Message = $"Transaction reference '{request.TransactionReference}' đã tồn tại." };
                }

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    var payment = new Models.Payment
                    {
                        OrderId = request.OrderId,
                        Amount = request.Amount,
                        PaymentMethod = request.PaymentMethod,
                        TransactionReference = request.TransactionReference,
                        SourceType = request.SourceType,
                        Status = request.Status,
                        PaidAt = request.PaymentDate,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.Payments.AddAsync(payment, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);

                    // Sau khi tạo payment thành công, tạo adoption và tree cho các order items có listingId
                    await CreateAdoptionsAndTreesFromOrderAsync(order, innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Tạo payment thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment for order {OrderId}", request.OrderId);
                return new AuthResponse { Success = false, Message = "Tạo payment thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> UpdatePaymentAsync(int paymentId, UpdatePaymentRequest request, CancellationToken ct = default)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, ct);
                if (payment == null)
                    return new AuthResponse { Success = false, Message = "Không tìm thấy paymentId: " + paymentId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    if (request.Amount.HasValue) payment.Amount = request.Amount.Value;
                    if (request.PaymentMethod != null) payment.PaymentMethod = request.PaymentMethod;
                    if (request.TransactionReference != null) payment.TransactionReference = request.TransactionReference;
                    if (request.SourceType != null) payment.SourceType = request.SourceType;
                    if (request.Status != null) payment.Status = request.Status;
                    if (request.PaymentDate.HasValue) payment.PaidAt = request.PaymentDate.Value;

                    _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Cập nhật payment thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment {PaymentId}", paymentId);
                return new AuthResponse { Success = false, Message = "Cập nhật payment thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> UpdatePaymentStatusAsync(int paymentId, UpdatePaymentStatusRequest request, CancellationToken ct = default)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, ct);
                if (payment == null)
                    return new AuthResponse { Success = false, Message = "Không tìm thấy paymentId: " + paymentId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    payment.Status = request.Status;
                    // Notes field not available on model; skip

                    _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Cập nhật trạng thái payment thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment status {PaymentId}", paymentId);
                return new AuthResponse { Success = false, Message = "Cập nhật trạng thái payment thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> DeletePaymentAsync(int paymentId, CancellationToken ct = default)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, ct);
                if (payment == null)
                    return new AuthResponse { Success = false, Message = "Không tìm thấy paymentId: " + paymentId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    // Soft delete by changing status
                    payment.Status = "CANCELLED";

                    _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Xóa payment thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete payment {PaymentId}", paymentId);
                return new AuthResponse { Success = false, Message = "Xóa payment thất bại: " + ex.Message };
            }
        }

        private async Task<PageResult<PaymentResponse>> MapPaymentsToResponseAsync(PageResult<Models.Payment> payments, CancellationToken ct)
        {
            var responses = new List<PaymentResponse>();
            foreach (var payment in payments.Items)
            {
                responses.Add(await MapPaymentToResponseAsync(payment, ct));
            }

            return new PageResult<PaymentResponse>
            {
                Items = responses,
                PageNumber = payments.PageNumber,
                PageSize = payments.PageSize,
                TotalItems = payments.TotalItems,
                TotalPages = payments.TotalPages
            };
        }

        private async Task<PaymentResponse> MapPaymentToResponseAsync(Models.Payment payment, CancellationToken ct)
        {
            return new PaymentResponse
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId ?? 0,
                Amount = payment.Amount,
                Currency = "VND", // default; not stored at model level
                PaymentMethod = payment.PaymentMethod,
                TransactionReference = payment.TransactionReference,
                SourceType = payment.SourceType,
                Status = payment.Status,
                PaymentDate = payment.PaidAt ?? payment.CreatedAt ?? DateTime.Now,
                Notes = null,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = null
            };
        }

        private async Task CreateAdoptionsAndTreesFromOrderAsync(Models.Order order, CancellationToken ct)
        {
            // Lấy tất cả order items có listingId (tree items)
            var treeOrderItems = order.OrderItems.Where(oi => oi.ListingId.HasValue).ToList();
            
            foreach (var orderItem in treeOrderItems)
            {
                if (!orderItem.ListingId.HasValue || !orderItem.TreeQuantity.HasValue) continue;

                var listingId = orderItem.ListingId.Value;
                var treeQuantity = orderItem.TreeQuantity.Value;
                var years = orderItem.TreeYears ?? 1;

                // Lấy listing một lần để dùng xuyên suốt
                var listing = await _unitOfWork.TreeListings.GetByIdAsync(listingId, includeTrees: false, ct);

                // Tạo tree cho từng cây được thuê
                for (int i = 0; i < treeQuantity; i++)
                {
                    // Generate unique code từ PostCode + ListingId + sequence
                    var treeCount = await _unitOfWork.Trees.GetCountByListingIdAsync(listingId, ct);
                    var postCode = listing?.PostCode ?? "LIST";
                    var uniqueCode = $"{postCode}_{listingId}_{treeCount + 1:D3}";

                    // Tạo tree trực tiếp trong database
                    var tree = new Models.Tree
                    {
                        ListingId = listingId,
                        UniqueCode = uniqueCode,
                        Description = $"Cây được tạo từ order {order.OrderId}",
                        Coordinates = "0,0", // Có thể cập nhật sau
                        HealthStatus = "HEALTHY",
                        AvailabilityStatus = "AVAILABLE",
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.Trees.AddAsync(tree, ct);
                    await _unitOfWork.SaveChangesAsync(ct);

                    // Giảm available quantity của tree listing
                    if (listing != null)
                    {
                        listing.AvailableQuantity -= 1;
                        _unitOfWork.TreeListings.UpdateAsync(listing);
                        await _unitOfWork.SaveChangesAsync(ct);
                    }

                    // Tạo adoption cho tree vừa tạo
                    var adoption = new Models.Adoption
                    {
                        CustomerId = order.CustomerId,
                        TreeId = tree.TreeId,
                        FarmerId = listing?.FarmerId ?? 0,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddYears(years),
                        Status = "ACTIVE",
                        OrderId = order.OrderId,
                        ProductName = listing?.Post?.ProductName ?? "Unknown Product",
                        Years = years,
                        PrimaryImageUrl = "",
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.Adoptions.AddAsync(adoption, ct);
                    await _unitOfWork.SaveChangesAsync(ct);
                }
            }
        }

        private async Task<PaymentDetailResponse> MapPaymentToDetailResponseAsync(Models.Payment payment, CancellationToken ct)
        {
            var order = payment.Order;
            var adoptions = await _unitOfWork.Adoptions.GetByOrderIdAsync(order?.OrderId ?? 0, 1, 100, ct);

            return new PaymentDetailResponse
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId ?? 0,
                Amount = payment.Amount,
                Currency = "VND",
                PaymentMethod = payment.PaymentMethod,
                TransactionReference = payment.TransactionReference,
                SourceType = payment.SourceType,
                Status = payment.Status,
                PaymentDate = payment.PaidAt ?? payment.CreatedAt ?? DateTime.Now,
                Notes = null,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = null,
                Order = order != null ? new OrderSummaryResponse
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer?.User?.FullName ?? "Unknown Customer",
                    SellerId = order.SellerId,
                    SellerName = order.Seller?.User?.FullName ?? "Unknown Seller",
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    CreatedAt = order.CreatedAt
                } : null,
                Adoptions = adoptions?.Items?.Select(a => new AdoptionSummaryResponse
                {
                    AdoptionId = a.AdoptionId,
                    TreeId = a.TreeId,
                    CustomerId = a.CustomerId,
                    CustomerName = a.Customer?.User?.FullName ?? "Unknown Customer",
                    TreeName = a.Tree?.Listing?.Post?.ProductName ?? "Unknown Tree",
                    Status = a.Status,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    CreatedAt = a.CreatedAt ?? DateTime.Now
                }).ToList() ?? new List<AdoptionSummaryResponse>()
            };
        }
    }
}

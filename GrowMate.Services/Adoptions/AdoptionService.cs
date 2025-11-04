using GrowMate.Contracts.Requests.Adoption;
using GrowMate.Contracts.Responses.Adoption;
using GrowMate.Contracts.Responses.Auth;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Extensions;
using Microsoft.Extensions.Logging;

namespace GrowMate.Services.Adoptions
{
    public interface IAdoptionService
    {
        Task<PageResult<CustomerAdoptionResponse>> GetCustomerAdoptionsAsync(int customerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<CustomerAdoptionResponse>> GetFarmerAdoptionsAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<CustomerAdoptionResponse>> GetAllAdoptionsAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<CustomerAdoptionResponse>> GetByOrderIdAsync(int orderId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<CustomerAdoptionResponse>> GetAdoptionByPostIdAsync(int postId, int page, int pageSize, CancellationToken ct = default);
        Task<CustomerAdoptionResponse?> GetAdoptionByIdAsync(int adoptionId, CancellationToken ct = default);
        Task<CustomerAdoptionResponse?> GetAdoptionDetailAsync(int adoptionId, CancellationToken ct = default);
        Task<AuthResponse> CreateAdoptionAsync(CreateAdoptionRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdateAdoptionAsync(int adoptionId, UpdateAdoptionRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdateAdoptionStatusAsync(int adoptionId, UpdateAdoptionStatusRequest request, CancellationToken ct = default);
        Task<AuthResponse> DeleteAdoptionAsync(int adoptionId, CancellationToken ct = default);
    }

    public class AdoptionService : IAdoptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdoptionService> _logger;

        public AdoptionService(IUnitOfWork unitOfWork, ILogger<AdoptionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PageResult<CustomerAdoptionResponse>> GetCustomerAdoptionsAsync(int customerId, int page, int pageSize, CancellationToken ct = default)
        {
            var adoptions = await _unitOfWork.Adoptions.GetByCustomerIdAsync(customerId, page, pageSize, ct);
            return await MapAdoptionsToResponseAsync(adoptions, ct);
        }

        public async Task<PageResult<CustomerAdoptionResponse>> GetFarmerAdoptionsAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var adoptions = await _unitOfWork.Adoptions.GetByFarmerIdAsync(farmerId, page, pageSize, ct);
            return await MapAdoptionsToResponseAsync(adoptions, ct);
        }

        public async Task<PageResult<CustomerAdoptionResponse>> GetAllAdoptionsAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var adoptions = await _unitOfWork.Adoptions.GetAllAsync(page, pageSize, ct);
            return await MapAdoptionsToResponseAsync(adoptions, ct);
        }

        public async Task<PageResult<CustomerAdoptionResponse>> GetByOrderIdAsync(int orderId, int page, int pageSize, CancellationToken ct = default)
        {
            var adoptions = await _unitOfWork.Adoptions.GetByOrderIdAsync(orderId, page, pageSize, ct);
            return await MapAdoptionsToResponseAsync(adoptions, ct);
        }

        public async Task<PageResult<CustomerAdoptionResponse>> GetAdoptionByPostIdAsync(int postId, int page, int pageSize, CancellationToken ct = default)
        {
            var adoptions = await _unitOfWork.Adoptions.GetByPostIdAsync(postId, page, pageSize, ct);
            return await MapAdoptionsToResponseAsync(adoptions, ct);
        }

        public async Task<CustomerAdoptionResponse?> GetAdoptionByIdAsync(int adoptionId, CancellationToken ct = default)
        {
            var adoption = await _unitOfWork.Adoptions.GetByIdAsync(adoptionId, ct);
            if (adoption == null) return null;

            return await MapAdoptionToResponseAsync(adoption, ct);
        }

        public async Task<CustomerAdoptionResponse?> GetAdoptionDetailAsync(int adoptionId, CancellationToken ct = default)
        {
            var adoption = await _unitOfWork.Adoptions.GetByIdWithDetailsAsync(adoptionId, ct);
            if (adoption == null) return null;

            return await MapAdoptionToResponseAsync(adoption, ct);
        }

        public async Task<AuthResponse> CreateAdoptionAsync(CreateAdoptionRequest request, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    var adoption = new Models.Adoption
                    {
                        CustomerId = request.CustomerId,
                        TreeId = request.TreeId,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        Status = "ACTIVE",
                        OrderId = request.OrderId,
                        PrimaryImageUrl = request.PrimaryImageUrl,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.Adoptions.AddAsync(adoption, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Tạo adoption thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create adoption for customer {CustomerId} and tree {TreeId}", request.CustomerId, request.TreeId);
                return new AuthResponse { Success = false, Message = "Tạo adoption thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> UpdateAdoptionAsync(int adoptionId, UpdateAdoptionRequest request, CancellationToken ct = default)
        {
            try
            {
                var adoption = await _unitOfWork.Adoptions.GetByIdAsync(adoptionId, ct);
                if (adoption == null) 
                    return new AuthResponse { Success = false, Message = "Không tìm thấy adoptionId: " + adoptionId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    adoption.StartDate = request.StartDate;
                    adoption.EndDate = request.EndDate;
                    adoption.PrimaryImageUrl = request.PrimaryImageUrl;

                    _unitOfWork.Adoptions.Update(adoption);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Cập nhật adoption thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update adoption {AdoptionId}", adoptionId);
                return new AuthResponse { Success = false, Message = "Cập nhật adoption thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> UpdateAdoptionStatusAsync(int adoptionId, UpdateAdoptionStatusRequest request, CancellationToken ct = default)
        {
            try
            {
                var adoption = await _unitOfWork.Adoptions.GetByIdAsync(adoptionId, ct);
                if (adoption == null) 
                    return new AuthResponse { Success = false, Message = "Không tìm thấy adoptionId: " + adoptionId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    adoption.Status = request.Status;

                    _unitOfWork.Adoptions.Update(adoption);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Cập nhật trạng thái adoption thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update adoption status {AdoptionId}", adoptionId);
                return new AuthResponse { Success = false, Message = "Cập nhật trạng thái adoption thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> DeleteAdoptionAsync(int adoptionId, CancellationToken ct = default)
        {
            try
            {
                var adoption = await _unitOfWork.Adoptions.GetByIdAsync(adoptionId, ct);
                if (adoption == null) 
                    return new AuthResponse { Success = false, Message = "Không tìm thấy adoptionId: " + adoptionId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    // Soft delete by changing status
                    adoption.Status = "CANCELLED";

                    _unitOfWork.Adoptions.Update(adoption);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Xóa adoption thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete adoption {AdoptionId}", adoptionId);
                return new AuthResponse { Success = false, Message = "Xóa adoption thất bại: " + ex.Message };
            }
        }

        private async Task<PageResult<CustomerAdoptionResponse>> MapAdoptionsToResponseAsync(PageResult<Models.Adoption> adoptions, CancellationToken ct)
        {
            var responses = new List<CustomerAdoptionResponse>();
            foreach (var adoption in adoptions.Items)
            {
                responses.Add(await MapAdoptionToResponseAsync(adoption, ct));
            }

            return new PageResult<CustomerAdoptionResponse>
            {
                Items = responses,
                PageNumber = adoptions.PageNumber,
                PageSize = adoptions.PageSize,
                TotalItems = adoptions.TotalItems,
                TotalPages = adoptions.TotalPages
            };
        }

        private async Task<CustomerAdoptionResponse> MapAdoptionToResponseAsync(Models.Adoption adoption, CancellationToken ct)
        {
            // Sử dụng các trường mới từ Adoption thay vì JOIN
            var primaryImageUrl = adoption.PrimaryImageUrl ?? "";
            var years = adoption.Years;
            var totalPrice = adoption.Tree.Listing.PricePerTree * years;

            return new CustomerAdoptionResponse
            {
                AdoptionId = adoption.AdoptionId,
                TreeId = adoption.TreeId,
                ListingId = adoption.Tree.ListingId,
                FarmerId = adoption.FarmerId, // Sử dụng trường mới
                TreeName = adoption.ProductName, // Sử dụng trường mới
                FarmerName = adoption.Tree.Listing.Farmer?.FarmName ?? "Unknown Farmer",
                UniqueCode = adoption.Tree.UniqueCode,
                Description = adoption.Tree.Description,
                Coordinates = adoption.Tree.Coordinates,
                HealthStatus = adoption.Tree.HealthStatus,
                AvailabilityStatus = adoption.Tree.AvailabilityStatus,
                StartDate = adoption.StartDate,
                EndDate = adoption.EndDate,
                Status = adoption.Status,
                CreatedAt = adoption.CreatedAt ?? DateTime.Now,
                PrimaryImageUrl = primaryImageUrl,
                PricePerYear = adoption.Tree.Listing.PricePerTree,
                Years = years, // Sử dụng trường mới
                TotalPrice = totalPrice,
                PostCode = adoption.Tree.Listing.PostCode, // Sử dụng PostCode từ Tree thay vì Listing
                PostId = adoption.Tree.Listing.PostId // Thêm PostId vào response
            };
        }
    }
}

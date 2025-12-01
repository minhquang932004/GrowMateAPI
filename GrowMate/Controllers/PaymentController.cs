using GrowMate.Contracts.Requests.Payment;
using GrowMate.Contracts.Responses.Payment;
using GrowMate.Services.Payments;
using GrowMate.Repositories.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Tạo QR thanh toán (SEPAY) cho order
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpPost("qr")]
        public async Task<IActionResult> CreateSepayQr([FromBody] CreateSepayQrRequest request)
        {
            if (request == null || request.OrderId <= 0)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new { Message = "Số tiền phải lớn hơn 0." });
            }

            // Thời gian hết hạn mặc định 10 phút ở service
            var qr = await _paymentService.CreateSepayQrAsync(request.OrderId, request.Amount, ct: HttpContext.RequestAborted);
            return Ok(qr);
        }

        /// <summary>
        /// Webhook từ Sepay (API Key qua header Authorization: Apikey ...)
        /// </summary>
        /// <remarks>AllowAnonymous để Sepay gọi.</remarks>
        [AllowAnonymous]
        [HttpPost("webhook/sepay")]
        public async Task<IActionResult> SepayWebhook()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            var auth = Request.Headers["Authorization"].ToString();

            var result = await _paymentService.ProcessSepayWebhookAsync(auth, body, HttpContext.RequestAborted);
            if (!result.Success)
            {
                // Nếu sai chứng thực -> trả 401 để từ chối
                if (string.Equals(result.Message, "Chứng thực webhook không hợp lệ.", StringComparison.Ordinal))
                {
                    return Unauthorized(new { success = false, message = result.Message });
                }
                // Các lỗi nghiệp vụ khác: trả 200 với success=false để Sepay không retry liên tục
                return Ok(new { success = false, message = result.Message });
            }
            // Sepay yêu cầu response có success: true và HTTP 200/201
            return Ok(new { success = true, message = result.Message });
        }

        /// <summary>
        /// Get payments with optional filters (paged)
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet]
        public async Task<IActionResult> GetPayments(
            [FromQuery] int? orderId = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? farmerId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            PageResult<PaymentResponse> payments;

            if (orderId.HasValue)
            {
                payments = await _paymentService.GetByOrderIdAsync(orderId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else if (customerId.HasValue)
            {
                payments = await _paymentService.GetByCustomerIdAsync(customerId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else if (farmerId.HasValue)
            {
                payments = await _paymentService.GetByFarmerIdAsync(farmerId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else
            {
                // Admin only - get all payments
                payments = await _paymentService.GetAllAsync(page, pageSize, HttpContext.RequestAborted);
            }

            return Ok(payments);
        }

        /// <summary>
        /// Get specific payment details
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentById(int paymentId)
        {
            var payment = await _paymentService.GetByIdAsync(paymentId, HttpContext.RequestAborted);
            if (payment == null)
            {
                return NotFound(new { Message = "Không tìm thấy payment." });
            }

            return Ok(payment);
        }

        /// <summary>
        /// Get payment detail with order and adoptions
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet("{paymentId}/detail")]
        public async Task<IActionResult> GetPaymentDetail(int paymentId)
        {
            var payment = await _paymentService.GetDetailAsync(paymentId, HttpContext.RequestAborted);
            if (payment == null)
            {
                return NotFound(new { Message = "Không tìm thấy payment." });
            }

            return Ok(payment);
        }

        /// <summary>
        /// Create new payment
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _paymentService.CreatePaymentAsync(request, HttpContext.RequestAborted);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Update payment
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpPut("{paymentId}")]
        public async Task<IActionResult> UpdatePayment(int paymentId, [FromBody] UpdatePaymentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _paymentService.UpdatePaymentAsync(paymentId, request, HttpContext.RequestAborted);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Update payment status
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpPut("{paymentId}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(int paymentId, [FromBody] UpdatePaymentStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _paymentService.UpdatePaymentStatusAsync(paymentId, request, HttpContext.RequestAborted);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Delete payment (soft delete)
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> DeletePayment(int paymentId)
        {
            var result = await _paymentService.DeletePaymentAsync(paymentId, HttpContext.RequestAborted);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}

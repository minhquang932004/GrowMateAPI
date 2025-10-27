using GrowMate.Contracts.Requests.Payment;
using GrowMate.Contracts.Responses.Payment;
using GrowMate.Services.Payments;
using GrowMate.Repositories.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

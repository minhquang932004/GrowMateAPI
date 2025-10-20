using GrowMate.Contracts.Requests.Adoption;
using GrowMate.Contracts.Responses.Adoption;
using GrowMate.Services.Adoptions;
using GrowMate.Services.Customers;
using GrowMate.Repositories.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdoptionController : ControllerBase
    {
        private readonly IAdoptionService _adoptionService;
        private readonly ICustomerService _customerService;

        public AdoptionController(IAdoptionService adoptionService, ICustomerService customerService)
        {
            _adoptionService = adoptionService;
            _customerService = customerService;
        }

        private async Task<int?> GetCurrentCustomerIdAsync()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            var customer = await _customerService.GetCustomerDetailsByIdAsync(userId, HttpContext.RequestAborted);
            if (customer == null)
            {
                return null;
            }

            return customer.CustomerId;
        }

        /// <summary>
        /// Get adoptions with optional filters (paged)
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet]
        public async Task<IActionResult> GetAdoptions(
            [FromQuery] int? customerId = null,
            [FromQuery] int? farmerId = null,
            [FromQuery] int? orderId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            PageResult<CustomerAdoptionResponse> adoptions;

            if (orderId.HasValue)
            {
                adoptions = await _adoptionService.GetByOrderIdAsync(orderId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else if (customerId.HasValue)
            {
                adoptions = await _adoptionService.GetCustomerAdoptionsAsync(customerId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else if (farmerId.HasValue)
            {
                adoptions = await _adoptionService.GetFarmerAdoptionsAsync(farmerId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else
            {
                adoptions = await _adoptionService.GetAllAdoptionsAsync(page, pageSize, HttpContext.RequestAborted);
            }

            return Ok(adoptions);
        }

        /// <summary>
        /// Get farmer's adoptions (paged) - DEPRECATED: Use GET /api/adoption?farmerId= instead
        /// </summary>
        /// <remarks>Role: Authenticated Farmer</remarks>
        //[HttpGet("farmer/{farmerId}")]
        //public async Task<IActionResult> GetFarmerAdoptions(int farmerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        //{
        //    var adoptions = await _adoptionService.GetFarmerAdoptionsAsync(farmerId, page, pageSize, HttpContext.RequestAborted);
        //    return Ok(adoptions);
        //}

        /// <summary>
        /// Get all adoptions (paged) - Admin only - DEPRECATED: Use GET /api/adoption instead
        /// </summary>
        /// <remarks>Role: Admin</remarks>
        //[HttpGet("all")]
        //public async Task<IActionResult> GetAllAdoptions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        //{
        //    var adoptions = await _adoptionService.GetAllAdoptionsAsync(page, pageSize, HttpContext.RequestAborted);
        //    return Ok(adoptions);
        //}

        /// <summary>
        /// Get specific adoption details
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet("{adoptionId}")]
        public async Task<IActionResult> GetAdoptionById(int adoptionId)
        {
            var adoption = await _adoptionService.GetAdoptionByIdAsync(adoptionId, HttpContext.RequestAborted);
            if (adoption == null)
            {
                return NotFound(new { Message = "Không tìm thấy adoption." });
            }

            return Ok(adoption);
        }

        /// <summary>
        /// Get adoption detail with reports and payments
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet("{adoptionId}/detail")]
        public async Task<IActionResult> GetAdoptionDetail(int adoptionId)
        {
            var adoption = await _adoptionService.GetAdoptionDetailAsync(adoptionId, HttpContext.RequestAborted);
            if (adoption == null)
            {
                return NotFound(new { Message = "Không tìm thấy adoption." });
            }

            return Ok(adoption);
        }

        /// <summary>
        /// Create new adoption
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpPost]
        public async Task<IActionResult> CreateAdoption([FromBody] CreateAdoptionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _adoptionService.CreateAdoptionAsync(request, HttpContext.RequestAborted);
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
        /// Update adoption
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpPut("{adoptionId}")]
        public async Task<IActionResult> UpdateAdoption(int adoptionId, [FromBody] UpdateAdoptionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _adoptionService.UpdateAdoptionAsync(adoptionId, request, HttpContext.RequestAborted);
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
        /// Update adoption status
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpPut("{adoptionId}/status")]
        public async Task<IActionResult> UpdateAdoptionStatus(int adoptionId, [FromBody] UpdateAdoptionStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _adoptionService.UpdateAdoptionStatusAsync(adoptionId, request, HttpContext.RequestAborted);
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
        /// Delete adoption (soft delete)
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpDelete("{adoptionId}")]
        public async Task<IActionResult> DeleteAdoption(int adoptionId)
        {
            var result = await _adoptionService.DeleteAdoptionAsync(adoptionId, HttpContext.RequestAborted);
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

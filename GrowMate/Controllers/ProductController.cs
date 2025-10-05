using GrowMate.Contracts.Requests;
using GrowMate.Models;
using GrowMate.Repositories.Models.Statuses;
using GrowMate.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrowMateWebAPIs.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // Public: list approved products for customers (paged) -> DTO
        [HttpGet("approved")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApproved([FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
        {
            var result = await _productService.GetApprovedListAsync(page, pageSize, ct);
            if (result.Items == null || result.Items.Count == 0)
                return NotFound("No approved products found.");
            return Ok(result);
        }

        // Admin: list pending products for review (entity list kept as-is)
        [HttpGet("pending")]
        [Authorize]
        public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken ct = default)
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "You are not allowed to do this function."
                });
            }

            var result = await _productService.GetPendingAsync(page, pageSize, ct);
            if (result.Items == null || result.Items.Count == 0)
                return NotFound("No pending products to review.");
            return Ok(result);
        }

        // Public: product details -> DTO
        // If not approved, only Admin can see it (customers get 404)
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int id, CancellationToken ct = default)
        {
            var dto = await _productService.GetDetailAsync(id, ct);
            if (dto == null)
                return NotFound($"ProductId {id} not found.");

            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            if (!isAdmin && !string.Equals(dto.Status, ProductStatuses.Approved, StringComparison.OrdinalIgnoreCase))
                return NotFound($"ProductId {id} not found."); // hide non-approved products from customers

            return Ok(dto);
        }

        // Farmer: create a product (auto PENDING) - unchanged
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct = default)
        {
            if (!User.IsInRole("Farmer"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "You are not allowed to do this function."
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = errors });
            }

            var product = new Product
            {
                FarmerId = request.FarmerId,
                CategoryId = request.CategoryId,
                ProductTypeId = request.ProductTypeId,
                UnitId = request.UnitId,
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock
                // Do NOT set Status, CreatedAt, UpdatedAt, or navigation properties here
            };

            var id = await _productService.CreateProductAsync(product, ct);
            return CreatedAtAction(nameof(GetProductById), new { id }, new { id });
        }

        // Admin: approve/reject/cancel product - unchanged
        [HttpPut("{id:int}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateProductStatus(int id, [FromQuery] string status, CancellationToken ct = default)
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "You are not allowed to do this function."
                });
            }

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ProductStatuses.Approved, ProductStatuses.Rejected, ProductStatuses.Canceled
            };
            if (!allowed.Contains(status))
                return BadRequest($"Invalid status. Allowed: {string.Join(", ", allowed)}");

            var ok = await _productService.UpdateProductStatusAsync(id, status, ct);
            if (!ok) return NotFound($"ProductId {id} not found.");
            return Ok(new { success = true, message = $"Updated product {id} to status {status}." });
        }
    }
}
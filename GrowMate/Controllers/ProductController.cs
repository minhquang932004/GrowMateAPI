using GrowMate.Contracts.Requests;
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

        /// <summary>
        /// List all approved products for customers (paged).
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
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

        /// <summary>
        /// List all pending products for admin review (paged).
        /// </summary>
        /// <remarks>Role: Admin only</remarks>
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

        /// <summary>
        /// Get product details by ID.
        /// </summary>
        /// <remarks>Role: Anonymous (only admin can see non-approved products)</remarks>
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

        /// <summary>
        /// Create a new product (auto PENDING).
        /// </summary>
        /// <remarks>Role: Farmer only</remarks>
        // Farmer: create a product (auto will be PENDING) - unchanged
        [HttpPost]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
        {
            var productId = await _productService.CreateProductAsync(request, ct);
            return Ok(new { ProductId = productId });
        }

        /// <summary>
        /// Update a product and its media.
        /// </summary>
        /// <remarks>Role: Farmer only</remarks>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductRequest request, CancellationToken ct)
        {
            var ok = await _productService.UpdateProductAsync(id, request, ct);
            if (!ok) return NotFound($"ProductId {id} not found.");
            return Ok(new { success = true, message = "Product updated." });
        }

        /// <summary>
        /// Delete a product by ID.
        /// </summary>
        /// <remarks>Role: Farmer or Admin</remarks>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Farmer,Admin")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct)
        {
            var ok = await _productService.DeleteProductAsync(id, ct);
            if (!ok) return NotFound($"ProductId {id} not found.");
            return Ok(new { success = true, message = "Product deleted." });
        }

        /// <summary>
        /// Update the status of a product (APPROVE/REJECT/CANCEL).
        /// </summary>
        /// <remarks>Role: Admin only</remarks>
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
using GrowMate.Contracts.Requests.Tree;
using GrowMate.Contracts.Responses.Tree;
using GrowMate.Services.Trees;
using GrowMate.Repositories.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TreeController : ControllerBase
    {
        private readonly ITreeService _treeService;

        public TreeController(ITreeService treeService)
        {
            _treeService = treeService;
        }

        /// <summary>
        /// Get trees with optional filters (paged)
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet]
        public async Task<IActionResult> GetTrees(
            [FromQuery] int? listingId = null,
            [FromQuery] int? farmerId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            PageResult<TreeResponse> trees;

            if (listingId.HasValue)
            {
                trees = await _treeService.GetByListingIdAsync(listingId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else if (farmerId.HasValue)
            {
                trees = await _treeService.GetByFarmerIdAsync(farmerId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else
            {
                trees = await _treeService.GetAllAsync(page, pageSize, HttpContext.RequestAborted);
            }

            return Ok(trees);
        }

        /// <summary>
        /// Get trees by listing ID (paged) - DEPRECATED: Use GET /api/tree?listingId= instead
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        //[HttpGet("listing/{listingId}")]
        //public async Task<IActionResult> GetTreesByListing(int listingId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        //{
        //    var trees = await _treeService.GetByListingIdAsync(listingId, page, pageSize, HttpContext.RequestAborted);
        //    return Ok(trees);
        //}

        /// <summary>
        /// Get farmer's trees (paged) - DEPRECATED: Use GET /api/tree?farmerId= instead
        /// </summary>
        /// <remarks>Role: Authenticated Farmer</remarks>
        //[HttpGet("farmer/{farmerId}")]
        //public async Task<IActionResult> GetFarmerTrees(int farmerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        //{
        //    var trees = await _treeService.GetByFarmerIdAsync(farmerId, page, pageSize, HttpContext.RequestAborted);
        //    return Ok(trees);
        //}

        /// <summary>
        /// Get all trees (paged) - Admin only - DEPRECATED: Use GET /api/tree instead
        /// </summary>
        /// <remarks>Role: Admin</remarks>
        //[HttpGet("all")]
        //public async Task<IActionResult> GetAllTrees([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        //{
        //    var trees = await _treeService.GetAllAsync(page, pageSize, HttpContext.RequestAborted);
        //    return Ok(trees);
        //}

        /// <summary>
        /// Get specific tree details
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet("{treeId}")]
        public async Task<IActionResult> GetTreeById(int treeId)
        {
            var tree = await _treeService.GetByIdAsync(treeId, HttpContext.RequestAborted);
            if (tree == null)
            {
                return NotFound(new { Message = "Không tìm thấy tree." });
            }

            return Ok(tree);
        }

        /// <summary>
        /// Get tree detail with adoptions
        /// </summary>
        /// <remarks>Role: Authenticated User</remarks>
        [HttpGet("{treeId}/detail")]
        public async Task<IActionResult> GetTreeDetail(int treeId)
        {
            var tree = await _treeService.GetDetailAsync(treeId, HttpContext.RequestAborted);
            if (tree == null)
            {
                return NotFound(new { Message = "Không tìm thấy tree." });
            }

            return Ok(tree);
        }

        /// <summary>
        /// Create new tree
        /// </summary>
        /// <remarks>Role: Authenticated Farmer</remarks>
        [HttpPost]
        public async Task<IActionResult> CreateTree([FromBody] CreateTreeRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _treeService.CreateTreeAsync(request, HttpContext.RequestAborted);
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
        /// Update tree
        /// </summary>
        /// <remarks>Role: Authenticated Farmer</remarks>
        [HttpPut("{treeId}")]
        public async Task<IActionResult> UpdateTree(int treeId, [FromBody] UpdateTreeRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _treeService.UpdateTreeAsync(treeId, request, HttpContext.RequestAborted);
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
        /// Update tree status
        /// </summary>
        /// <remarks>Role: Authenticated Farmer</remarks>
        [HttpPut("{treeId}/status")]
        public async Task<IActionResult> UpdateTreeStatus(int treeId, [FromBody] UpdateTreeStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            var result = await _treeService.UpdateTreeStatusAsync(treeId, request, HttpContext.RequestAborted);
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
        /// Delete tree (soft delete)
        /// </summary>
        /// <remarks>Role: Authenticated Farmer</remarks>
        [HttpDelete("{treeId}")]
        public async Task<IActionResult> DeleteTree(int treeId)
        {
            var result = await _treeService.DeleteTreeAsync(treeId, HttpContext.RequestAborted);
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

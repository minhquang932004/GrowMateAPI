using GrowMate.Services.TreeListings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrowMateWebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreeListingController : ControllerBase
    {
        private readonly ITreeListingService _treeListingService;

        public TreeListingController(ITreeListingService treeListingService)
        {
            _treeListingService = treeListingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTreeListing([FromQuery] int? treeListingId, [FromQuery] int? farmerId, [FromQuery] int? postId, [FromQuery] bool includeTree = false , [FromQuery] int page = 1, [FromQuery] int pageSize = 3 ,CancellationToken ct = default)
        {
            if (treeListingId.HasValue)
            {
                var item = await _treeListingService.GetByIdAsync(treeListingId.Value, includeTree, ct);
                if (item == null) return NotFound("Không tìm thấy treeListingId: " + treeListingId);
                return Ok(item);
            }
            if (farmerId.HasValue)
            {
                var item = await _treeListingService.GetByFarmerIdAsync(farmerId.Value, page, pageSize, ct);
                if (item.Items == null || item.Items.Count == 0)
                    return NotFound("Không tìm thấy các bài đăng của farmerId: " + farmerId);
                return Ok(item);
            }
            if (postId.HasValue)
            {
                var item = await _treeListingService.GetByPostIdAsync(postId.Value, includeTree,ct);
                if (item == null) return NotFound("Không tìm thấy postId: " + postId);
                return Ok(item);
            }
            var allTreeListing = await _treeListingService.GetAllAsync(page, pageSize, ct);
            if(allTreeListing.Items == null || allTreeListing.Items.Count == 0)
            {
                return NotFound(allTreeListing);
            }
            return Ok(allTreeListing);
        }
    }
}

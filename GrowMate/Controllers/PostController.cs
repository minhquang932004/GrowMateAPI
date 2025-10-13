using GrowMate.Contracts.Requests;
using GrowMate.Services.Farmers;
using GrowMate.Services.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using GrowMate.Contracts.Requests.Post;

namespace GrowMateWebAPIs.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IFarmerService _farmerService;

        public PostController(IPostService postService, IFarmerService farmerService)
        {
            _postService = postService;
            _farmerService = farmerService;
        }

        /// <summary>
        /// Get all posts, or filter by postId or farmerId.
        /// </summary>
        /// <remarks>Role: Anonymous (anyone can access)</remarks>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPosts([FromQuery] int? postId, [FromQuery] int? farmerId,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 3, CancellationToken ct = default)
        {
            if (postId.HasValue)
            {
                var item = await _postService.GetPostByIdAsync(postId.Value, ct);
                if (item == null) return NotFound("Không tìm thấy postId: " + postId);
                return Ok(item);
            }

            if (farmerId.HasValue)
            {
                var farmerCheck = await _farmerService.GetFarmerByIdAsync(farmerId.Value);
                if (!farmerCheck) return NotFound("Không tìm thấy farmerId: " + farmerId);

                var item = await _postService.GetAllPostsByFarmerIdAsync(farmerId.Value, page, pageSize, ct);
                if (item.Items == null || item.Items.Count == 0)
                    return NotFound("Không tìm thấy các bài đăng của farmerId: " + farmerId);
                return Ok(item);
            }

            var allPosts = await _postService.GetAllPostsAsync(page, pageSize, ct);
            if (allPosts.Items == null || allPosts.Items.Count == 0)
                return NotFound("Không tìm thấy bài đăng nào!!!");
            return Ok(allPosts);
        }

        /// <summary>
        /// Create a new post.
        /// </summary>
        /// <remarks>Role: Farmer only</remarks>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request, CancellationToken ct)
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
            var result = await _postService.CreatePostAsync(request, ct);
            if (result.Success) return CreatedAtAction(nameof(CreatePost), result);
            return BadRequest(result);
        }

        /// <summary>
        /// Delete a post by ID.
        /// </summary>
        /// <remarks>Role: Farmer or Admin</remarks>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id, CancellationToken ct)
        {
            if (!User.IsInRole("Farmer") && !User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "You are not allowed to do this function."
                });
            }

            var result = await _postService.DeletePostAsync(id, ct);
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Update a post by ID (Include its Media).
        /// </summary>
        /// <remarks>Role: Farmer or Admin</remarks>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] CreatePostRequest request, CancellationToken ct)
        {
            if (!User.IsInRole("Farmer") && !User.IsInRole("Admin"))
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
            var result = await _postService.UpdatePostAsync(id, request, ct);
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Update the status of a post (APPROVE/REJECT/CANCEL).
        /// </summary>
        /// <remarks>Role: Admin only</remarks>
        [HttpPut("{id:int}/status")]
        [Authorize]
        public async Task<IActionResult> UpdatePostStatus(int id, [FromQuery] string status, CancellationToken ct = default)
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "You are not allowed to do this function."
                });
            }

            var result = await _postService.UpdatePostStatusAsync(id, status, ct);
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }
    }
}
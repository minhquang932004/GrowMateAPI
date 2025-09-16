using GrowMate.DTOs.Requests;
using GrowMate.Services.Farmers;
using GrowMate.Services.Posts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
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

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = errors });
            }
            var result = await _postService.CreatePostAsync(request);
            if (result.Success)
            {
                return CreatedAtAction(nameof(CreatePost), result);
            }
            return BadRequest(result);

        }
        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] int? postId, [FromQuery] int? farmerId,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 3)
        {
            if (postId.HasValue)
            {
                var item = await _postService.GetPostByIdAsync(postId.Value);
                if (item == null)
                {
                    return NotFound("Không tìm thấy postId: " + postId);
                }
                return Ok(item);
            }
            if (farmerId.HasValue)
            {
                var farmerCheck = await _farmerService.GetFarmerByIdAsync(farmerId.Value);
                if (!farmerCheck)
                {
                    return NotFound("Không tìm thấy farmerId: " + farmerId);
                }
                var item = await _postService.GetAllPostsByFarmerIdAsync(farmerId.Value, page, pageSize);
                if (item.Items == null || item.Items.Count == 0)
                {
                    return NotFound("Không tìm thấy các bài đăng của farmerId: " + farmerId);
                }
                return Ok(item);
            }
            var allPosts = await _postService.GetAllPostsAsync(page, pageSize);
            if (allPosts.Items == null || allPosts.Items.Count == 0)
            {
                return NotFound("Không tìm thấy bài đăng nào!!!");
            }
            return Ok(allPosts);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var result = await _postService.DeletePostAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] CreatePostRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = errors });
            }
            var result = await _postService.UpdatePostAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdatePostStatus(int id, [FromQuery] string status)
        {
            var result = await _postService.UpdatePostStatusAsync(id, status);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}

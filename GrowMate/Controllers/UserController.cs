using GrowMate.DTOs.Requests;
using GrowMate.Services.UserAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserAccountService _userAccountService;

        public UserController(IUserAccountService userAccountService)
        {
            _userAccountService = userAccountService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserByAdmin([FromBody] CreateUserByAdminRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = errors });
            }
            var emailExist = await _userAccountService.GetUserByEmailAsync(request.Email);
            if (emailExist != null)
            {
                return BadRequest(new { Message = "Email đã tồn tại!!" });
            }
            var phoneExist = await _userAccountService.GetUserByPhoneAsync(request.Phone);
            if (phoneExist != null)
            {
                return BadRequest(new { Message = "Số điện thoại đã được đăng kí" });
            }
            var result = await _userAccountService.CreateUserByAdminAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);

        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int? id, [FromQuery] string? email, [FromQuery] string? phone, [FromQuery] int page = 1, [FromQuery] int pageSize = 3)
        {
            if (id.HasValue)
            {
                var user = await _userAccountService.GetUserByIdAsync(id.Value);
                if (user == null)
                {
                    return NotFound("Không tìm thấy userId: " + id.Value);
                }
                return Ok(user);
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await _userAccountService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound("Không tìm thấy user có email: " + email);
                }
                return Ok(user);
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var user = await _userAccountService.GetUserByPhoneAsync(phone);
                if (user == null)
                {
                    return NotFound("Không tìm thấy user có số điện thoại: " + phone);
                }
                return Ok(user);
            }
            var userList = await _userAccountService.GetAllUserAsync(page, pageSize);
            if (userList.Items == null || userList.Items.Count == 0)
            {
                return NotFound("Không tìm thấy danh sách user!!!");
            }
            return Ok(userList);
        }
    }
}

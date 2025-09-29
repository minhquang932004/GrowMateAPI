using GrowMate.Contracts.Requests;
using GrowMate.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GrowMate.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userAccountService;

        public UserController(IUserService userAccountService)
        {
            _userAccountService = userAccountService;
        }

        // ADMIN: create arbitrary users
        [HttpPost("by-admin")]
        [Authorize]
        public async Task<IActionResult> CreateUserByAdmin([FromBody] CreateUserByAdminRequest request, CancellationToken ct)
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    sucess = false,
                    message = "You are not allowed to do this function."
                });
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = errors });
            }
            var emailExist = await _userAccountService.GetUserByEmailAsync(request.Email, false, ct);
            if (emailExist != null)
            {
                return BadRequest(new { Message = "Email đã tồn tại!!" });
            }
            var result = await _userAccountService.CreateUserByAdminAsync(request, ct);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // READ:
        // - Admin can query by any criteria (id/email/list)
        // - Non-admin can only read their own profile by id
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsers([FromQuery] int? id, [FromQuery] string? email, [FromQuery] string? phone, [FromQuery] bool includeCustomer = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 3, CancellationToken ct = default)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = GetCurrentUserId();

            if (!isAdmin)
            {
                // Non-admin can only fetch themselves by id
                if (!id.HasValue || currentUserId is null || id.Value != currentUserId.Value)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        success = false,
                        message = "You are not allowed to do this function."
                    });
                }

                var self = await _userAccountService.GetUserByIdAsync(id.Value, includeCustomer, ct);
                if (self == null)
                {
                    return NotFound("Không tìm thấy userId: " + id.Value);
                }
                return Ok(self);
            }

            // Admin branch: allow all existing behaviors
            if (id.HasValue)
            {
                var user = await _userAccountService.GetUserByIdAsync(id.Value, includeCustomer, ct);
                if (user == null)
                {
                    return NotFound("Không tìm thấy userId: " + id.Value);
                }
                return Ok(user);
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await _userAccountService.GetUserByEmailAsync(email, includeCustomer, ct);
                if (user == null)
                {
                    return NotFound("Không tìm thấy user có email: " + email);
                }
                return Ok(user);
            }

            var userList = await _userAccountService.GetAllUserAsync(page, pageSize, ct);
            if (userList.Items == null || userList.Items.Count == 0)
            {
                return NotFound("Không tìm thấy danh sách user!!!");
            }
            return Ok(userList);
        }

        // SELF or ADMIN: update
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = errors });
            }

            var isAdmin = User.IsInRole("Admin");
            var currentUserId = GetCurrentUserId();
            if (!isAdmin && (currentUserId is null || id != currentUserId.Value))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "You are not allowed to do this function."
                });
            }

            var result = await _userAccountService.UpdateUserAsync(id, request, ct);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // ADMIN: update any user (role/active etc.)
        [HttpPut("by-admin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserByAdmin(int id, [FromBody] UpdateUserByAdminRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = errors });
            }
            var result = await _userAccountService.UpdateUserByAdminAsync(id, request, ct);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // ADMIN: delete user
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
        {
            var result = await _userAccountService.DeleteUserAsync(id, ct);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        // SELF or ADMIN: change password
        [HttpPut("user-password/{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserPassword(int id, UpdateUserPwdRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = errors });
            }

            var isAdmin = User.IsInRole("Admin");
            var currentUserId = GetCurrentUserId();
            if (!isAdmin && (currentUserId is null || id != currentUserId.Value))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "You are not allowed to do this function."
                });
            }

            var result = await _userAccountService.UpdateUserPasswordAsync(id, request, ct);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        private int? GetCurrentUserId()
        {
            // Prefer sub (JWT) then NameIdentifier
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (int.TryParse(sub, out var id))
                return id;
            // Some token issuers put user id only in "sub"
            var rawSub = User.FindFirstValue("sub");
            return int.TryParse(rawSub, out id) ? id : null;
        }
    }
}

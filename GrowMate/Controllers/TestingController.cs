using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestingController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
            => Ok("Hello World! This is a public endpoint.");

        // Requires a valid Bearer token
        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirst("email")?.Value; // set in TokenService via JwtRegisteredClaimNames.Email
            var name = User.Identity?.Name;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

            return Ok(new
            {
                message = "You are authenticated.",
                user = new
                {
                    userId,
                    email,
                    name,
                    roles
                }
            });
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestingController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
            => Ok("Hello World! This is a public endpoint.");
    }
}

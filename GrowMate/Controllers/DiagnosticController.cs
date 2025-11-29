using Microsoft.AspNetCore.Mvc;

namespace GrowMateWebAPIs.Controllers
{
    [ApiController]
    [Route("api/diag")]
    public class DiagnosticController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(IHttpClientFactory httpClientFactory, ILogger<DiagnosticController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("network-check")]
        public async Task<IActionResult> NetworkCheck()
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                using var resp = await client.GetAsync("https://www.google.com");
                return Ok(new { reachable = true, status = (int)resp.StatusCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NetworkCheck failed");
                return Problem(detail: ex.ToString(), statusCode: 500);
            }
        }
    }
}

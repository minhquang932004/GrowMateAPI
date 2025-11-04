using GrowMate.Contracts.Requests.MonthlyReport;
using GrowMate.Contracts.Responses.Auth;
using GrowMate.Contracts.Responses.MonthlyReport;
using GrowMate.Repositories.Extensions;
using GrowMate.Services.MonthlyReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer,Farmer")]
    public class MonthlyReportController : ControllerBase
    {
        private readonly IMonthlyReportService _monthlyReportService;

        public MonthlyReportController(IMonthlyReportService monthlyReportService)
        {
            _monthlyReportService = monthlyReportService;
        }

        /// <summary>
        /// Get monthly reports with optional filters (paged)
        /// </summary>
        /// <remarks>
        /// Role: Customer, Farmer
        /// 
        /// Filter priority:
        /// 1. If both adoptionId and status are provided, filter by both
        /// 2. If only adoptionId is provided, filter by adoptionId
        /// 3. If only status is provided, filter by status
        /// 4. If only farmerId is provided, filter by farmerId
        /// 5. Otherwise, return all reports
        /// 
        /// Allowed status values: DRAFT, PUBLISHED, REMOVED
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetMonthlyReports(
            [FromQuery] int? adoptionId = null,
            [FromQuery] int? farmerId = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            PageResult<MonthlyReportResponse> reports;

            // Priority: adoptionId + status > adoptionId > status > farmerId > all
            if (adoptionId.HasValue && !string.IsNullOrEmpty(status))
            {
                reports = await _monthlyReportService.GetByAdoptionIdAndStatusAsync(adoptionId.Value, status, page, pageSize, HttpContext.RequestAborted);
            }
            else if (adoptionId.HasValue)
            {
                reports = await _monthlyReportService.GetByAdoptionIdAsync(adoptionId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else if (!string.IsNullOrEmpty(status))
            {
                reports = await _monthlyReportService.GetByStatusAsync(status, page, pageSize, HttpContext.RequestAborted);
            }
            else if (farmerId.HasValue)
            {
                reports = await _monthlyReportService.GetByFarmerIdAsync(farmerId.Value, page, pageSize, HttpContext.RequestAborted);
            }
            else
            {
                reports = await _monthlyReportService.GetAllMonthlyReportsAsync(page, pageSize, HttpContext.RequestAborted);
            }

            return Ok(reports);
        }

        /// <summary>
        /// Get specific monthly report by ID
        /// </summary>
        /// <remarks>Role: Customer, Farmer</remarks>
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetMonthlyReportById(int reportId)
        {
            var report = await _monthlyReportService.GetByIdAsync(reportId, HttpContext.RequestAborted);
            if (report == null)
            {
                return NotFound(new { Message = "Không tìm thấy monthly report." });
            }

            return Ok(report);
        }

        /// <summary>
        /// Create new monthly report
        /// </summary>
        /// <remarks>Role: Farmer</remarks>
        [HttpPost]
        public async Task<IActionResult> CreateMonthlyReport([FromBody] CreateMonthlyReportRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            if (!User.IsInRole("Farmer"))
            {
                return StatusCode(403, new { success = false, message = "You are not allowed to do this function." });
            }

            var result = await _monthlyReportService.CreateMonthlyReportAsync(request, HttpContext.RequestAborted);
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
        /// Update monthly report
        /// </summary>
        /// <remarks>Role: Farmer</remarks>
        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateMonthlyReport(int reportId, [FromBody] UpdateMonthlyReportRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Dữ liệu request không hợp lệ." });
            }

            if (!User.IsInRole("Farmer"))
            {
                return StatusCode(403, new { success = false, message = "You are not allowed to do this function." });
            }

            var result = await _monthlyReportService.UpdateMonthlyReportAsync(reportId, request, HttpContext.RequestAborted);
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
        /// Update monthly report status
        /// </summary>
        /// <remarks>
        /// Role: Farmer
        /// 
        /// Allowed status values:
        /// - DRAFT: Report is in draft mode, not visible to customers
        /// - PUBLISHED: Report is published and visible to customers
        /// - REMOVED: Report is removed/hidden from view
        /// </remarks>
        /// <param name="reportId">The ID of the monthly report to update</param>
        /// <param name="request">Request body containing the new status (DRAFT, PUBLISHED, or REMOVED)</param>
        [HttpPut("{reportId}/status")]
        public async Task<IActionResult> UpdateMonthlyReportStatus(int reportId, [FromBody] UpdateMonthlyReportStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
            {
                return BadRequest(new { Message = "Status không được để trống." });
            }

            if (!User.IsInRole("Farmer"))
            {
                return StatusCode(403, new { success = false, message = "You are not allowed to do this function." });
            }

            var result = await _monthlyReportService.UpdateMonthlyReportStatusAsync(reportId, request.Status, HttpContext.RequestAborted);
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
        /// Delete monthly report
        /// </summary>
        /// <remarks>Role: Farmer</remarks>
        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteMonthlyReport(int reportId)
        {
            if (!User.IsInRole("Farmer"))
            {
                return StatusCode(403, new { success = false, message = "You are not allowed to do this function." });
            }

            var result = await _monthlyReportService.DeleteMonthlyReportAsync(reportId, HttpContext.RequestAborted);
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


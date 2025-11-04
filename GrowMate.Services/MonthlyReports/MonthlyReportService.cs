using GrowMate.Contracts.Requests.MonthlyReport;
using GrowMate.Contracts.Responses.Auth;
using GrowMate.Contracts.Responses.MonthlyReport;
using GrowMate.Models;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models.Statuses;
using GrowMate.Services.Media;
using Microsoft.Extensions.Logging;

namespace GrowMate.Services.MonthlyReports
{
    public class MonthlyReportService : IMonthlyReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediaService _mediaService;
        private readonly ILogger<MonthlyReportService> _logger;

        public MonthlyReportService(
            IUnitOfWork unitOfWork,
            IMediaService mediaService,
            ILogger<MonthlyReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _mediaService = mediaService;
            _logger = logger;
        }

        public async Task<PageResult<MonthlyReportResponse>> GetAllMonthlyReportsAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var reports = await _unitOfWork.MonthlyReports.GetAllAsync(page, pageSize, ct);
            return await MapReportsToResponseAsync(reports, ct);
        }

        public async Task<PageResult<MonthlyReportResponse>> GetByAdoptionIdAsync(int adoptionId, int page, int pageSize, CancellationToken ct = default)
        {
            var reports = await _unitOfWork.MonthlyReports.GetByAdoptionIdAsync(adoptionId, page, pageSize, ct);
            return await MapReportsToResponseAsync(reports, ct);
        }

        public async Task<PageResult<MonthlyReportResponse>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var reports = await _unitOfWork.MonthlyReports.GetByFarmerIdAsync(farmerId, page, pageSize, ct);
            return await MapReportsToResponseAsync(reports, ct);
        }

        public async Task<PageResult<MonthlyReportResponse>> GetByStatusAsync(string status, int page, int pageSize, CancellationToken ct = default)
        {
            var reports = await _unitOfWork.MonthlyReports.GetByStatusAsync(status, page, pageSize, ct);
            return await MapReportsToResponseAsync(reports, ct);
        }

        public async Task<PageResult<MonthlyReportResponse>> GetByAdoptionIdAndStatusAsync(int adoptionId, string status, int page, int pageSize, CancellationToken ct = default)
        {
            var reports = await _unitOfWork.MonthlyReports.GetByAdoptionIdAndStatusAsync(adoptionId, status, page, pageSize, ct);
            return await MapReportsToResponseAsync(reports, ct);
        }

        public async Task<MonthlyReportResponse?> GetByIdAsync(int reportId, CancellationToken ct = default)
        {
            var report = await _unitOfWork.MonthlyReports.GetByIdAsync(reportId, ct);
            if (report == null) return null;

            return MapReportToResponseAsync(report, ct);
        }

        public async Task<AuthResponse> CreateMonthlyReportAsync(CreateMonthlyReportRequest request, CancellationToken ct = default)
        {
            try
            {
                // Check if report already exists for this adoption, month, and year
                var existingReport = await _unitOfWork.MonthlyReports.GetByAdoptionIdAndMonthYearAsync(
                    request.AdoptionId, request.ReportMonth, request.ReportYear, ct);
                
                if (existingReport != null)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = $"Monthly report for adoption {request.AdoptionId} in {request.ReportMonth}/{request.ReportYear} already exists" 
                    };
                }

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    var newReport = new Models.MonthlyReport
                    {
                        AdoptionId = request.AdoptionId,
                        FarmerId = request.FarmerId,
                        ReportMonth = request.ReportMonth,
                        ReportYear = request.ReportYear,
                        Activities = request.Activities,
                        Notes = request.Notes,
                        Status = MonthlyReportStatuses.Draft,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.MonthlyReports.AddAsync(newReport, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);

                    // Create media if provided
                    if (request.Media != null && request.Media.Count > 0)
                    {
                        await _mediaService.CreateMediaAsync(request.Media, reportId: newReport.ReportId, ct: innerCt);
                    }
                }, ct);

                return new AuthResponse { Success = true, Message = "Tạo monthly report thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create monthly report for adoption {AdoptionId}", request.AdoptionId);
                return new AuthResponse { Success = false, Message = "Tạo monthly report thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> UpdateMonthlyReportAsync(int reportId, UpdateMonthlyReportRequest request, CancellationToken ct = default)
        {
            try
            {
                var report = await _unitOfWork.MonthlyReports.GetByIdAsync(reportId, ct);
                if (report == null)
                    return new AuthResponse { Success = false, Message = "Không tìm thấy reportId: " + reportId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    report.Activities = request.Activities;
                    report.Notes = request.Notes;
                    
                    if (!string.IsNullOrEmpty(request.Status))
                    {
                        report.Status = request.Status;
                        if (request.Status.ToLower() == "published" && report.PublishedAt == null)
                        {
                            report.PublishedAt = DateTime.Now;
                        }
                    }

                    _unitOfWork.MonthlyReports.Update(report);

                    // Replace media if provided
                    if (request.Media != null)
                    {
                        await _mediaService.ReplaceReportMediaAsync(reportId, request.Media, innerCt);
                    }

                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Cập nhật monthly report thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update monthly report {ReportId}", reportId);
                return new AuthResponse { Success = false, Message = "Cập nhật monthly report thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> UpdateMonthlyReportStatusAsync(int reportId, string status, CancellationToken ct = default)
        {
            try
            {
                // Validate status
                if (status != MonthlyReportStatuses.Draft && 
                    status != MonthlyReportStatuses.Published && 
                    status != MonthlyReportStatuses.Removed)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = $"Status không hợp lệ. Chỉ chấp nhận: {MonthlyReportStatuses.Draft}, {MonthlyReportStatuses.Published}, {MonthlyReportStatuses.Removed}" 
                    };
                }

                var report = await _unitOfWork.MonthlyReports.GetByIdAsync(reportId, ct);
                if (report == null)
                    return new AuthResponse { Success = false, Message = "Không tìm thấy reportId: " + reportId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    report.Status = status;
                    
                    // Set PublishedAt when status changes to Published
                    if (status == MonthlyReportStatuses.Published && report.PublishedAt == null)
                    {
                        report.PublishedAt = DateTime.Now;
                    }

                    _unitOfWork.MonthlyReports.Update(report);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Cập nhật status thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update monthly report status {ReportId} to {Status}", reportId, status);
                return new AuthResponse { Success = false, Message = "Cập nhật status thất bại: " + ex.Message };
            }
        }

        public async Task<AuthResponse> DeleteMonthlyReportAsync(int reportId, CancellationToken ct = default)
        {
            try
            {
                var report = await _unitOfWork.MonthlyReports.GetByIdAsync(reportId, ct);
                if (report == null)
                    return new AuthResponse { Success = false, Message = "Không tìm thấy reportId: " + reportId };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    // Delete associated media first
                    var media = await _unitOfWork.Media.GetByReportIdAsync(reportId, innerCt);
                    if (media != null && media.Count > 0)
                    {
                        _unitOfWork.Media.RemoveRange(media);
                    }

                    _unitOfWork.MonthlyReports.Remove(report);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                }, ct);

                return new AuthResponse { Success = true, Message = "Xóa monthly report thành công!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete monthly report {ReportId}", reportId);
                return new AuthResponse { Success = false, Message = "Xóa monthly report thất bại: " + ex.Message };
            }
        }

        private Task<PageResult<MonthlyReportResponse>> MapReportsToResponseAsync(PageResult<Models.MonthlyReport> reports, CancellationToken ct)
        {
            var responses = reports.Items.Select(report => MapReportToResponseAsync(report, ct)).ToList();

            return Task.FromResult(new PageResult<MonthlyReportResponse>
            {
                Items = responses,
                PageNumber = reports.PageNumber,
                PageSize = reports.PageSize,
                TotalItems = reports.TotalItems,
                TotalPages = reports.TotalPages
            });
        }

        private MonthlyReportResponse MapReportToResponseAsync(Models.MonthlyReport report, CancellationToken ct)
        {
            return new MonthlyReportResponse
            {
                ReportId = report.ReportId,
                AdoptionId = report.AdoptionId,
                FarmerId = report.FarmerId,
                ReportMonth = report.ReportMonth,
                ReportYear = report.ReportYear,
                Activities = report.Activities,
                Notes = report.Notes,
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                PublishedAt = report.PublishedAt,
                Media = report.Media?.Select(m => new MediaReportResponse
                {
                    MediaId = m.MediaId,
                    ReportId = m.ReportId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType,
                    IsPrimary = m.IsPrimary,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                }).ToList() ?? new List<MediaReportResponse>()
            };
        }
    }
}


using GrowMate.Contracts.Requests.MonthlyReport;
using GrowMate.Contracts.Responses.Auth;
using GrowMate.Contracts.Responses.MonthlyReport;
using GrowMate.Repositories.Extensions;

namespace GrowMate.Services.MonthlyReports
{
    public interface IMonthlyReportService
    {
        Task<PageResult<MonthlyReportResponse>> GetAllMonthlyReportsAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<MonthlyReportResponse>> GetByAdoptionIdAsync(int adoptionId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<MonthlyReportResponse>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<MonthlyReportResponse>> GetByStatusAsync(string status, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<MonthlyReportResponse>> GetByAdoptionIdAndStatusAsync(int adoptionId, string status, int page, int pageSize, CancellationToken ct = default);
        Task<MonthlyReportResponse?> GetByIdAsync(int reportId, CancellationToken ct = default);
        Task<AuthResponse> CreateMonthlyReportAsync(CreateMonthlyReportRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdateMonthlyReportAsync(int reportId, UpdateMonthlyReportRequest request, CancellationToken ct = default);
        Task<AuthResponse> UpdateMonthlyReportStatusAsync(int reportId, string status, CancellationToken ct = default);
        Task<AuthResponse> DeleteMonthlyReportAsync(int reportId, CancellationToken ct = default);
    }
}


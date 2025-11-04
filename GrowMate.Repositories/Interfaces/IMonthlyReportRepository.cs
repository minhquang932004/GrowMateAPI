using GrowMate.Models;
using GrowMate.Repositories.Extensions;

namespace GrowMate.Repositories.Interfaces
{
    public interface IMonthlyReportRepository
    {
        Task AddAsync(MonthlyReport report, CancellationToken ct = default);
        void Update(MonthlyReport report);
        void Remove(MonthlyReport report);

        Task<MonthlyReport?> GetByIdAsync(int reportId, CancellationToken ct = default);
        Task<PageResult<MonthlyReport>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<MonthlyReport>> GetByAdoptionIdAsync(int adoptionId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<MonthlyReport>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<MonthlyReport>> GetByStatusAsync(string status, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<MonthlyReport>> GetByAdoptionIdAndStatusAsync(int adoptionId, string status, int page, int pageSize, CancellationToken ct = default);
        Task<MonthlyReport?> GetByAdoptionIdAndMonthYearAsync(int adoptionId, int month, int year, CancellationToken ct = default);
        Task<bool> ExistsAsync(int reportId, CancellationToken ct = default);
    }
}


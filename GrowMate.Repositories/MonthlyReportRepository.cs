using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models.Statuses;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class MonthlyReportRepository : IMonthlyReportRepository
    {
        private readonly EXE201_GrowMateContext _dbContext;

        public MonthlyReportRepository(EXE201_GrowMateContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(MonthlyReport report, CancellationToken ct = default)
        {
            await _dbContext.MonthlyReports.AddAsync(report, ct);
        }

        public void Update(MonthlyReport report)
        {
            _dbContext.MonthlyReports.Update(report);
        }

        public void Remove(MonthlyReport report)
        {
            _dbContext.MonthlyReports.Remove(report);
        }

        public async Task<MonthlyReport?> GetByIdAsync(int reportId, CancellationToken ct = default)
        {
            return await _dbContext.MonthlyReports
                .Include(r => r.Adoption)
                .Include(r => r.Farmer)
                .Include(r => r.Media)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReportId == reportId, ct);
        }

        public async Task<PageResult<MonthlyReport>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbContext.MonthlyReports
                .AsNoTracking()
                .Include(r => r.Adoption)
                .Include(r => r.Farmer)
                .Include(r => r.Media)
                .OrderByDescending(r => r.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<MonthlyReport>> GetByAdoptionIdAsync(int adoptionId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbContext.MonthlyReports
                .AsNoTracking()
                .Include(r => r.Adoption)
                .Include(r => r.Farmer)
                .Include(r => r.Media)
                .Where(r => r.AdoptionId == adoptionId)
                .OrderByDescending(r => r.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<MonthlyReport>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbContext.MonthlyReports
                .AsNoTracking()
                .Include(r => r.Adoption)
                .Include(r => r.Farmer)
                .Include(r => r.Media)
                .Where(r => r.FarmerId == farmerId)
                .OrderByDescending(r => r.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<MonthlyReport>> GetByStatusAsync(string status, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbContext.MonthlyReports
                .AsNoTracking()
                .Include(r => r.Adoption)
                .Include(r => r.Farmer)
                .Include(r => r.Media)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<MonthlyReport>> GetByAdoptionIdAndStatusAsync(int adoptionId, string status, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbContext.MonthlyReports
                .AsNoTracking()
                .Include(r => r.Adoption)
                .Include(r => r.Farmer)
                .Include(r => r.Media)
                .Where(r => r.AdoptionId == adoptionId && r.Status == status)
                .OrderByDescending(r => r.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<MonthlyReport?> GetByAdoptionIdAndMonthYearAsync(int adoptionId, int month, int year, CancellationToken ct = default)
        {
            return await _dbContext.MonthlyReports
                .AsNoTracking()
                .Include(r => r.Adoption)
                .Include(r => r.Farmer)
                .Include(r => r.Media)
                .FirstOrDefaultAsync(r => r.AdoptionId == adoptionId && r.ReportMonth == month && r.ReportYear == year, ct);
        }

        public async Task<bool> ExistsAsync(int reportId, CancellationToken ct = default)
        {
            return await _dbContext.MonthlyReports.AnyAsync(r => r.ReportId == reportId, ct);
        }
    }
}


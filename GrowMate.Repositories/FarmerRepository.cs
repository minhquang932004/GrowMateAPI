using GrowMate.Repositories.Data;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class FarmerRepository : IFarmerRepository
    {
        private readonly EXE201_GrowMateContext _dbContext;

        public FarmerRepository(EXE201_GrowMateContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
            => _dbContext.Farmers.AsNoTracking().AnyAsync(f => f.FarmerId == id, ct);

        public Task<Farmer?> GetByIdAsync(int id, CancellationToken ct = default)
            => _dbContext.Farmers.AsNoTracking().FirstOrDefaultAsync(f => f.FarmerId == id, ct);
    }
}
using GrowMate.Models;
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

        public async Task CreateAsync(Farmer farmer, CancellationToken ct = default)
        {
            await _dbContext.Farmers.AddAsync(farmer, ct);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
            => _dbContext.Farmers.AsNoTracking().AnyAsync(f => f.FarmerId == id, ct);

        public Task<Farmer?> GetByIdAsync(int id, CancellationToken ct = default)
            => _dbContext.Farmers.AsNoTracking().FirstOrDefaultAsync(f => f.FarmerId == id, ct);

        public Task<Farmer?> GetByUserIdAsync(int userId, CancellationToken ct = default)
            => _dbContext.Farmers.AsNoTracking().FirstOrDefaultAsync(f => f.UserId == userId, ct);

        public Task<bool> ExistsByUserIdAsync(int userId, CancellationToken ct = default)
            => _dbContext.Farmers.AsNoTracking().AnyAsync(f => f.UserId == userId, ct);

        public async Task Remove(Farmer farmer)
        {
            var item = _dbContext.Farmers.Remove(farmer);
            await _dbContext.SaveChangesAsync();
        }

        public void UpdateAsync(Farmer farmer)
        {
            _dbContext.Farmers.Update(farmer);
        }
    }
}
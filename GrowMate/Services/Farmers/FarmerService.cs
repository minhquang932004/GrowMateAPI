
using GrowMate.Data;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Services.Farmers
{
    public class FarmerService : IFarmerService
    {
        private readonly EXE201_GrowMateContext _context;
        public FarmerService(EXE201_GrowMateContext context)
        {
            _context = context;
        }
        public async Task<bool> GetFarmerByIdAsync(int id)
        {
            var item = await _context.Farmers.AsNoTracking().FirstOrDefaultAsync(a => a.FarmerId == id);
            if(item == null)
            {
                return false;
            }
            return true;
        }
    }
}

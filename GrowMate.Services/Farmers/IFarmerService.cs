namespace GrowMate.Services.Farmers
{
    public interface IFarmerService
    {
        Task<bool> GetFarmerByIdAsync(int id);
    }
}

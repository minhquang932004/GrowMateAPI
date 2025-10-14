using GrowMate.Contracts.Requests.Customer;
using GrowMate.Models;


namespace GrowMate.Services.Customers
{
    public interface ICustomerService
    {
        Task<bool> GetCustomerByIdAsync(int id);
        Task<Customer> GetCustomerDetailsByIdAsync(int id, CancellationToken ct);
        Task UpdateCustomerAsync(int id, CustomerRequest request, CancellationToken ct);
        Task RemoveByUserIdAsync(int userId, CancellationToken ct);
        Task CreateByUserId(int userId, CustomerRequest? request, CancellationToken ct);
    }
}

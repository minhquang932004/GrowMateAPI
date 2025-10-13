using GrowMate.Contracts.Requests;
using GrowMate.Contracts.Requests.Customer; // Add the domain-specific namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Services.Customers
{
    public interface ICustomerService
    {
        Task<bool> GetCustomerByIdAsync(int id);

        Task UpdateCustomerAsync(int id, CustomerRequest request, CancellationToken ct);

        Task RemoveByUserIdAsync(int userId, CancellationToken ct);

        Task CreateByUserId(int userId, CustomerRequest? request, CancellationToken ct);
    }
}

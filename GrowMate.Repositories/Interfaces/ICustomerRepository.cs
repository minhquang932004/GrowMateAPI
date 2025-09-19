using GrowMate.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<bool> AnyAsync(int userId, CancellationToken ct = default);
        Task CreateAsync(Customer customer, CancellationToken ct = default);
    }
}

using GrowMate.Models;
using GrowMate.Repositories.Extensions;

namespace GrowMate.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<PageResult<Payment>> GetByOrderIdAsync(int orderId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Payment>> GetByCustomerIdAsync(int customerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Payment>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default);
        Task<PageResult<Payment>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<Payment?> GetByIdAsync(int paymentId, CancellationToken ct = default);
        Task<Payment?> GetByIdWithDetailsAsync(int paymentId, CancellationToken ct = default);
        Task AddAsync(Payment payment, CancellationToken ct = default);
        void Update(Payment payment);
        void Remove(Payment payment);
        Task<bool> ExistsAsync(int paymentId, CancellationToken ct = default);
        Task<bool> ExistsByTransactionReferenceAsync(string transactionReference, CancellationToken ct = default);
        Task<Payment?> GetByGatewayOrderCodeAsync(string gatewayOrderCode, CancellationToken ct = default);
        Task<Payment?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken ct = default);
    }
}

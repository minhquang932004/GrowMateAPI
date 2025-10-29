using GrowMate.Models;
using GrowMate.Repositories.Data;
using GrowMate.Repositories.Extensions;
using GrowMate.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrowMate.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly EXE201_GrowMateContext _db;

        public PaymentRepository(EXE201_GrowMateContext db)
        {
            _db = db;
        }

        public async Task<PageResult<Payment>> GetByOrderIdAsync(int orderId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(p => p.Order)
                .ThenInclude(o => o.Seller)
                .ThenInclude(s => s.User)
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Payment>> GetByCustomerIdAsync(int customerId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(p => p.Order)
                .ThenInclude(o => o.Seller)
                .ThenInclude(s => s.User)
                .Where(p => p.Order.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Payment>> GetByFarmerIdAsync(int farmerId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(p => p.Order)
                .ThenInclude(o => o.Seller)
                .ThenInclude(s => s.User)
                .Where(p => p.Order.SellerId == farmerId)
                .OrderByDescending(p => p.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<PageResult<Payment>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(p => p.Order)
                .ThenInclude(o => o.Seller)
                .ThenInclude(s => s.User)
                .OrderByDescending(p => p.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, ct);
        }

        public async Task<Payment?> GetByIdAsync(int paymentId, CancellationToken ct = default)
        {
            return await _db.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(p => p.Order)
                .ThenInclude(o => o.Seller)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct);
        }

        public async Task<Payment?> GetByIdWithDetailsAsync(int paymentId, CancellationToken ct = default)
        {
            return await _db.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(p => p.Order)
                .ThenInclude(o => o.Seller)
                .ThenInclude(s => s.User)
                .Include(p => p.Order)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct);
        }

        public async Task AddAsync(Payment payment, CancellationToken ct = default)
        {
            await _db.Payments.AddAsync(payment, ct);
        }

        public void Update(Payment payment)
        {
            _db.Payments.Update(payment);
        }

        public void Remove(Payment payment)
        {
            _db.Payments.Remove(payment);
        }

        public async Task<bool> ExistsAsync(int paymentId, CancellationToken ct = default)
        {
            return await _db.Payments.AnyAsync(p => p.PaymentId == paymentId, ct);
        }

        public async Task<bool> ExistsByTransactionReferenceAsync(string transactionReference, CancellationToken ct = default)
        {
            return await _db.Payments.AnyAsync(p => p.TransactionReference == transactionReference, ct);
        }

        public async Task<Payment?> GetByGatewayOrderCodeAsync(string gatewayOrderCode, CancellationToken ct = default)
        {
            return await _db.Payments
                .FirstOrDefaultAsync(p => p.GatewayOrderCode == gatewayOrderCode, ct);
        }

        public async Task<Payment?> GetByTransactionReferenceAsync(string transactionReference, CancellationToken ct = default)
        {
            return await _db.Payments
                .FirstOrDefaultAsync(p => p.TransactionReference == transactionReference, ct);
        }
    }
}

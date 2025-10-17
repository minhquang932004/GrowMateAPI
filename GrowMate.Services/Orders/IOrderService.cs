using GrowMate.Models;

namespace GrowMate.Services.Orders
{
    public interface IOrderService
    {
        Task<Order> CreateOrderFromCartAsync(int cartId, string? shippingAddress = null, string? notes = null);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order?>> GetOrdersByCustomerIdAsync(int customerId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, string? reason = null);
        Task<bool> UpdatePaymentStatusAsync(int orderId, string paymentStatus, string? transactionId = null);
    }
}

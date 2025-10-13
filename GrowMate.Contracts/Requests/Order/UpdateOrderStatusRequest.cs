using GrowMate.Repositories.Models.Statuses;

namespace GrowMate.Contracts.Requests.Order
{
    /// <summary>
    /// Request model for updating an order's status
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// The new status for the order
        /// Valid values: OrderStatuses.Pending, OrderStatuses.Processing, OrderStatuses.Shipped,
        /// OrderStatuses.Delivered, OrderStatuses.Completed, OrderStatuses.Cancelled, OrderStatuses.Refunded
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Optional reason for the status change
        /// </summary>
        public string Reason { get; set; }
    }
}
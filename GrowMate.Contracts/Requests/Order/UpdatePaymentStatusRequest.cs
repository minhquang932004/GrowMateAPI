using GrowMate.Repositories.Models.Statuses;
using System;

namespace GrowMate.Contracts.Requests.Order
{
    /// <summary>
    /// Request model for updating an order's payment status
    /// </summary>
    public class UpdatePaymentStatusRequest
    {
        /// <summary>
        /// The new payment status
        /// Valid values: PaymentStatuses.Pending, PaymentStatuses.Completed, PaymentStatuses.Failed,
        /// PaymentStatuses.Refunded, PaymentStatuses.PartiallyRefunded, PaymentStatuses.OnHold
        /// </summary>
        public string PaymentStatus { get; set; }
        
        /// <summary>
        /// Optional transaction identifier from payment provider
        /// </summary>
        public string TransactionId { get; set; }
        
        /// <summary>
        /// Optional payment date
        /// </summary>
        public DateTime? PaymentDate { get; set; }
    }
}
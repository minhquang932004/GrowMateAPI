namespace GrowMate.Contracts.Responses.Customer
{
    /// <summary>
    /// Response model representing customer information
    /// </summary>
    public class CustomerResponse
    {
        /// <summary>
        /// The unique identifier for the customer
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// Customer's shipping address for deliveries
        /// </summary>
        public string? ShippingAddress { get; set; }
        
        /// <summary>
        /// Customer's wallet balance (in VND)
        /// </summary>
        public decimal? WalletBalance { get; set; }
    }
}
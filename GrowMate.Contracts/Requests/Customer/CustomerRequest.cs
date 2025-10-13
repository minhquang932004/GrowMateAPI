namespace GrowMate.Contracts.Requests.Customer
{
    /// <summary>
    /// Request model for customer-specific information when creating or updating a user
    /// </summary>
    public class CustomerRequest
    {
        /// <summary>
        /// Optional shipping address for deliveries
        /// </summary>
        public string? ShippingAddress { get; set; }
        
        /// <summary>
        /// Optional wallet balance amount (in VND)
        /// </summary>
        public decimal? WalletBalance { get; set; }
    }
}
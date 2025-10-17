namespace GrowMate.Contracts.Requests.Order
{
    /// <summary>
    /// Request model for creating an order from a shopping cart
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// Optional shipping address information
        /// </summary>
        public string ShippingAddress { get; set; }

        /// <summary>
        /// Optional additional notes for the order
        /// </summary>
        public string Notes { get; set; }
    }
}
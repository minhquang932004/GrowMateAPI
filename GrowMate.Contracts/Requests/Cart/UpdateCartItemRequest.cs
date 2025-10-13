namespace GrowMate.Contracts.Requests.Cart
{
    /// <summary>
    /// Request model for updating a cart item's quantity
    /// </summary>
    public class UpdateCartItemRequest
    {
        /// <summary>
        /// The new quantity for the cart item
        /// </summary>
        public int Quantity { get; set; }
    }
}
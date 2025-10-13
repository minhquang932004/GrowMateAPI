namespace GrowMate.Contracts.Requests.Cart
{
    /// <summary>
    /// Request model for adding an item to the cart
    /// </summary>
    public class AddToCartRequest
    {
        /// <summary>
        /// The ID of the product to add to the cart
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// The quantity of the product to add
        /// </summary>
        public int Quantity { get; set; }
    }
}
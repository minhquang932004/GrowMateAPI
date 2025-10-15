namespace GrowMate.Contracts.Requests.Cart
{
    /// <summary>
    /// Request model for adding a tree listing to the cart (adoption flow)
    /// </summary>
    public class AddTreeToCartRequest
    {
        /// <summary>
        /// The ID of the tree listing to add
        /// </summary>
        public int ListingId { get; set; }

        /// <summary>
        /// Number of trees to add
        /// </summary>
        public int Quantity { get; set; }
    }
}



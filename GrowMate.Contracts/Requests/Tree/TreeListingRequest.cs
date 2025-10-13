namespace GrowMate.Contracts.Requests.Tree
{
    /// <summary>
    /// Request model for creating or updating a tree listing
    /// </summary>
    public class TreeListingRequest
    {
        /// <summary>
        /// The identifier of the post this tree listing is associated with
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// The identifier of the farmer who is creating this listing
        /// </summary>
        public int FarmerId { get; set; }

        /// <summary>
        /// Price per individual tree in the listing (in VND)
        /// </summary>
        public decimal PricePerTree { get; set; }

        /// <summary>
        /// Total number of trees in this listing
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// Number of trees that are available for purchase
        /// </summary>
        public int AvailableQuantity { get; set; }
    }
}
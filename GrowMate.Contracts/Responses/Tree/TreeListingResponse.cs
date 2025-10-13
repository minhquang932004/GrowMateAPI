using System;
using System.Collections.Generic;

namespace GrowMate.Contracts.Responses.Tree
{
    /// <summary>
    /// Response model representing a listing of trees associated with a post
    /// </summary>
    public class TreeListingResponse
    {
        /// <summary>
        /// The unique identifier for the tree listing
        /// </summary>
        public int ListingId { get; set; }

        /// <summary>
        /// The identifier of the post this tree listing is associated with
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// The identifier of the farmer who created this listing
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
        /// Number of trees that are still available for purchase
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// Current status of the listing (e.g., Active, Inactive)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The date and time when the listing was created
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the listing was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// List of individual trees in this listing
        /// </summary>
        public List<TreeResponse>? TreeResponses { get; set; }
    }
}
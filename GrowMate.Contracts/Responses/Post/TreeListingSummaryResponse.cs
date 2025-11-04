using System;

namespace GrowMate.Contracts.Responses.Post
{
    /// <summary>
    /// Simplified TreeListing response for use within PostResponse (excludes redundant fields like postId, farmerId, pricePerTree)
    /// </summary>
    public class TreeListingSummaryResponse
    {
        /// <summary>
        /// The unique identifier for the tree listing
        /// </summary>
        public int ListingId { get; set; }

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
    }
}


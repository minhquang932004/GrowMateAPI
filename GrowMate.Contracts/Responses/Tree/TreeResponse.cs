using System;

namespace GrowMate.Contracts.Responses.Tree
{
    /// <summary>
    /// Response model representing a specific tree in a tree listing
    /// </summary>
    public class TreeResponse
    {
        /// <summary>
        /// The unique identifier for the tree
        /// </summary>
        public int TreeId { get; set; }

        /// <summary>
        /// The identifier of the listing this tree belongs to
        /// </summary>
        public int ListingId { get; set; }

        /// <summary>
        /// A unique code identifying this specific tree
        /// </summary>
        public string UniqueCode { get; set; }

        /// <summary>
        /// Detailed description of the tree
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Geographic coordinates where the tree is located
        /// </summary>
        public string Coordinates { get; set; }

        /// <summary>
        /// Current health status of the tree
        /// </summary>
        public string HealthStatus { get; set; }

        /// <summary>
        /// Current availability status of the tree (e.g., Available, Reserved, Sold)
        /// </summary>
        public string AvailabilityStatus { get; set; }

        /// <summary>
        /// The date and time when the tree record was created
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
}
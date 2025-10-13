using System.Collections.Generic;
using GrowMate.Contracts.Requests.Media;

namespace GrowMate.Contracts.Requests.Post
{
    /// <summary>
    /// Request model for creating a new post by a farmer
    /// </summary>
    public class CreatePostRequest
    {
        /// <summary>
        /// The identifier of the farmer creating this post
        /// </summary>
        public int FarmerId { get; set; }

        /// <summary>
        /// The name of the product being posted
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// The type of product (e.g., fruit, vegetable, etc.)
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// The specific variety of the product
        /// </summary>
        public string ProductVariety { get; set; }

        /// <summary>
        /// The name of the farm where the product is grown
        /// </summary>
        public string FarmName { get; set; }

        /// <summary>
        /// The geographic origin of the product
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// The price per year for sponsorship or adoption
        /// </summary>
        public decimal PricePerYear { get; set; }

        /// <summary>
        /// Expected harvest weight per cycle
        /// </summary>
        public decimal? HarvestWeight { get; set; }

        /// <summary>
        /// The unit of measurement for the harvest (e.g., kg, lb)
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Number of harvests per year
        /// </summary>
        public int? HarvestFrequency { get; set; }

        /// <summary>
        /// The number of trees or plants available
        /// </summary>
        public int TreeQuantity { get; set; }

        /// <summary>
        /// Detailed description of the post/product
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of media items (images/videos) for this post
        /// Note: This is named "Media" to match the database (not "Medium")
        /// </summary>
        public List<MediaItemRequest>? Media { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace GrowMate.Contracts.Responses.Post
{
    /// <summary>
    /// Response model representing a post created by a farmer
    /// </summary>
    public class PostResponse
    {
        /// <summary>
        /// The unique identifier for the post
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// The identifier of the farmer who created this post
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
        /// The current status of the post (Pending, Approved, Rejected)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The date and time when the post was created
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the post was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// List of media items (images/videos) associated with this post
        /// </summary>
        public List<MediaPostResponse>? MediaPostList { get; set; }
        
        /// <summary>
        /// List of comments on this post
        /// </summary>
        public List<PostCommentResponse>? PostCommentList { get; set; } 
    }
}
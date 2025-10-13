using GrowMate.Contracts.Responses.Media;
using System.Collections.Generic;

namespace GrowMate.Contracts.Responses.Product
{
    /// <summary>
    /// Response model representing detailed information about a product
    /// </summary>
    public sealed class ProductDetailResponse
    {
        /// <summary>
        /// The unique identifier for the product
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// The identifier of the farmer who produced this product
        /// </summary>
        public int FarmerId { get; set; }
        
        /// <summary>
        /// The name of the product
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// URL-friendly version of the product name
        /// </summary>
        public string Slug { get; set; }
        
        /// <summary>
        /// Detailed description of the product
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// The price of the product (in VND)
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Available quantity in stock
        /// </summary>
        public int Stock { get; set; }
        
        /// <summary>
        /// The current status of the product (Active, Inactive, etc.)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The name of the category this product belongs to
        /// </summary>
        public string CategoryName { get; set; }
        
        /// <summary>
        /// The type of product (e.g., fruit, vegetable, etc.)
        /// </summary>
        public string ProductTypeName { get; set; }
        
        /// <summary>
        /// The unit of measurement for the product (e.g., kg, bundle, etc.)
        /// </summary>
        public string UnitName { get; set; }
        
        /// <summary>
        /// The name of the farmer who produced this product
        /// </summary>
        public string FarmerName { get; set; }

        /// <summary>
        /// List of media items (images/videos) associated with this product
        /// Note: This is named "Media" to match the database name, not "Medium"
        /// </summary>
        public List<MediaResponse> Media { get; set; } = new();
        
        /// <summary>
        /// URL to the main image of the product
        /// </summary>
        public string MainImageUrl { get; set; }
    }
}
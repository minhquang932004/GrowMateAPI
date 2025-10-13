using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using GrowMate.Contracts.Requests.Media;

namespace GrowMate.Contracts.Requests.Product
{
    /// <summary>
    /// Request model for creating a new product
    /// </summary>
    public class CreateProductRequest
    {
        /// <summary>
        /// The identifier of the farmer creating this product
        /// </summary>
        [Required]
        public int FarmerId { get; set; }
        
        /// <summary>
        /// Optional category identifier for this product
        /// </summary>
        public int? CategoryId { get; set; }
        
        /// <summary>
        /// Optional product type identifier
        /// </summary>
        public int? ProductTypeId { get; set; }
        
        /// <summary>
        /// Optional unit of measurement identifier
        /// </summary>
        public int? UnitId { get; set; }
        
        /// <summary>
        /// Name of the product
        /// </summary>
        [Required]
        public string Name { get; set; }
        
        /// <summary>
        /// URL-friendly version of the product name
        /// </summary>
        [Required]
        public string Slug { get; set; }
        
        /// <summary>
        /// Detailed description of the product
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Price of the product (in VND)
        /// </summary>
        [Required]
        public decimal Price { get; set; }
        
        /// <summary>
        /// Available quantity in stock
        /// </summary>
        [Required]
        public int Stock { get; set; }

        /// <summary>
        /// List of media items (images/videos) for this product
        /// Note: This is named "Media" to match the database (not "Medium")
        /// </summary>
        public List<MediaItemRequest> Media { get; set; }
    }
}
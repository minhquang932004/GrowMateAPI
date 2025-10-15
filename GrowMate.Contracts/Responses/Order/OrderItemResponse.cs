using System;
using System.Text.Json.Serialization;

namespace GrowMate.Contracts.Responses.Order
{
    /// <summary>
    /// Base response model representing an item in an order
    /// This is used as a union type to handle both ProductOrderItemResponse and TreeOrderItemResponse
    /// </summary>
    [JsonDerivedType(typeof(ProductOrderItemResponse), typeDiscriminator: "Product")]
    [JsonDerivedType(typeof(TreeOrderItemResponse), typeDiscriminator: "Tree")]
    public abstract class OrderItemResponse
    {
        /// <summary>
        /// The unique identifier for the order item
        /// </summary>
        public int OrderItemId { get; set; }
        
        /// <summary>
        /// The identifier of the order this item belongs to
        /// </summary>
        public int OrderId { get; set; }
        
        /// <summary>
        /// The name of the product/tree
        /// </summary>
        public string ProductName { get; set; }
        
        /// <summary>
        /// The date and time when the item was added to the order
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Optional product/tree image URL
        /// </summary>
        public string ProductImageUrl { get; set; }
        
        /// <summary>
        /// The type of order item (Product or Tree)
        /// </summary>
        public abstract string ItemType { get; }
    }
}
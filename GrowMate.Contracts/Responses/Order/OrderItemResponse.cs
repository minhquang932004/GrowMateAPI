using GrowMate.Contracts.Utils;
using System;

namespace GrowMate.Contracts.Responses.Order
{
    /// <summary>
    /// Response model representing an item in an order
    /// </summary>
    public class OrderItemResponse
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
        /// The identifier of the product
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// The name of the product
        /// </summary>
        public string ProductName { get; set; }
        
        /// <summary>
        /// The quantity of the product ordered
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// The unit price of the product at the time of ordering (in VND)
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// The unit price formatted as a VND currency string
        /// </summary>
        public string UnitPriceFormatted => CurrencyUtils.FormatVND(UnitPrice);
        
        /// <summary>
        /// The total price for this order item (quantity * unit price) in VND
        /// </summary>
        public decimal TotalPrice => Quantity * UnitPrice;
        
        /// <summary>
        /// The total price formatted as a VND currency string
        /// </summary>
        public string TotalPriceFormatted => CurrencyUtils.FormatVND(TotalPrice);
        
        /// <summary>
        /// The date and time when the item was added to the order
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Optional product image URL
        /// </summary>
        public string ProductImageUrl { get; set; }
    }
}
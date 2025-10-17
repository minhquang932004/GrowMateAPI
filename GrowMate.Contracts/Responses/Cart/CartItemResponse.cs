using GrowMate.Contracts.Utils;
using System;

namespace GrowMate.Contracts.Responses.Cart
{
    /// <summary>
    /// Response model representing an item in a shopping cart
    /// </summary>
    public class CartItemResponse
    {
        /// <summary>
        /// The unique identifier for the cart item
        /// </summary>
        public int CartItemId { get; set; }
        
        /// <summary>
        /// The identifier of the cart this item belongs to
        /// </summary>
        public int CartId { get; set; }
        
        /// <summary>
        /// The identifier of the product
        /// </summary>
        public int ProductId { get; set; }
        public int? ListingId { get; set; }
        
        /// <summary>
        /// The name of the product
        /// </summary>
        public string ProductName { get; set; }
        
        /// <summary>
        /// The quantity of the product in the cart
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// The unit price of the product at the time it was added to the cart (in VND)
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// The unit price formatted as a VND currency string
        /// </summary>
        public string UnitPriceFormatted => CurrencyUtils.FormatVND(UnitPrice);
        
        /// <summary>
        /// The total price for this cart item (quantity * unit price) in VND
        /// </summary>
        public decimal TotalPrice => Quantity * UnitPrice;
        
        /// <summary>
        /// The total price formatted as a VND currency string
        /// </summary>
        public string TotalPriceFormatted => CurrencyUtils.FormatVND(TotalPrice);
        
        /// <summary>
        /// The date and time when the item was added to the cart
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Optional product image URL
        /// </summary>
        public string ProductImageUrl { get; set; }

        // Tree listing (adoption) fields reuse existing names
        // Quantity + UnitPrice will be used for tree items via mapping in controller
        
        /// <summary>
        /// Optional: number of years for tree listings; null for product items
        /// </summary>
        public int? Years { get; set; }
    }
}
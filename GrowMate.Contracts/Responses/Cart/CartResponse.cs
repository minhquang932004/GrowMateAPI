using GrowMate.Contracts.Utils;
using System;
using System.Collections.Generic;

namespace GrowMate.Contracts.Responses.Cart
{
    /// <summary>
    /// Response model representing a customer's shopping cart
    /// </summary>
    public class CartResponse
    {
        /// <summary>
        /// The unique identifier for the cart
        /// </summary>
        public int CartId { get; set; }
        
        /// <summary>
        /// The identifier of the customer who owns the cart
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// The current status of the cart (Active, Ordered, etc.)
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// The date and time when the cart was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// The date and time when the cart was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// The list of items in the cart
        /// </summary>
        public List<CartItemResponse> CartItems { get; set; } = new List<CartItemResponse>();
        
        /// <summary>
        /// The total number of items in the cart
        /// </summary>
        public int TotalItems => CartItems?.Count ?? 0;
        
        /// <summary>
        /// The subtotal of all items in the cart (in VND)
        /// </summary>
        public decimal Subtotal
        {
            get
            {
                decimal total = 0;
                if (CartItems != null)
                {
                    foreach (var item in CartItems)
                    {
                        total += item.Quantity * item.UnitPrice;
                    }
                }
                return total;
            }
        }
        
        /// <summary>
        /// The subtotal formatted as a VND currency string
        /// </summary>
        public string SubtotalFormatted => CurrencyUtils.FormatVND(Subtotal);
    }
}
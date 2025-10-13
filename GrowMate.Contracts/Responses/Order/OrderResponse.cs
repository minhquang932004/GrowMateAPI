using GrowMate.Contracts.Utils;
using System;
using System.Collections.Generic;

namespace GrowMate.Contracts.Responses.Order
{
    /// <summary>
    /// Response model representing an order
    /// </summary>
    public class OrderResponse
    {
        /// <summary>
        /// The unique identifier for the order
        /// </summary>
        public int OrderId { get; set; }
        
        /// <summary>
        /// The identifier of the customer who placed the order
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// The customer's name for display purposes
        /// </summary>
        public string CustomerName { get; set; }
        
        /// <summary>
        /// The identifier of the farmer/seller
        /// </summary>
        public int SellerId { get; set; }
        
        /// <summary>
        /// The farmer/seller name for display purposes
        /// </summary>
        public string SellerName { get; set; }
        
        /// <summary>
        /// The current status of the order
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// The current payment status
        /// </summary>
        public string PaymentStatus { get; set; }
        
        /// <summary>
        /// The currency used for this order (VND)
        /// </summary>
        public string Currency { get; set; } = "VND";
        
        /// <summary>
        /// Subtotal of all items in the order (in VND)
        /// </summary>
        public decimal Subtotal { get; set; }
        
        /// <summary>
        /// Subtotal formatted as a VND currency string
        /// </summary>
        public string SubtotalFormatted => CurrencyUtils.FormatVND(Subtotal);
        
        /// <summary>
        /// Shipping fee for the order (in VND)
        /// </summary>
        public decimal ShippingFee { get; set; }
        
        /// <summary>
        /// Shipping fee formatted as a VND currency string
        /// </summary>
        public string ShippingFeeFormatted => CurrencyUtils.FormatVND(ShippingFee);
        
        /// <summary>
        /// Platform fee for the order (in VND) - this is the commission the platform takes
        /// </summary>
        public decimal PlatformFee { get; set; }
        
        /// <summary>
        /// Platform fee formatted as a VND currency string
        /// </summary>
        public string PlatformFeeFormatted => CurrencyUtils.FormatVND(PlatformFee);
        
        /// <summary>
        /// Total amount for the order (subtotal + shipping fee) in VND
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Total amount formatted as a VND currency string
        /// </summary>
        public string TotalAmountFormatted => CurrencyUtils.FormatVND(TotalAmount);
        
        /// <summary>
        /// Amount that will be paid to the seller after platform fee is deducted
        /// </summary>
        public decimal SellerAmount => Subtotal - PlatformFee;
        
        /// <summary>
        /// Seller amount formatted as a VND currency string
        /// </summary>
        public string SellerAmountFormatted => CurrencyUtils.FormatVND(SellerAmount);
        
        /// <summary>
        /// Optional shipping address information
        /// </summary>
        public string ShippingAddress { get; set; }
        
        /// <summary>
        /// Optional additional notes for the order
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// The payment method used (if any)
        /// </summary>
        public string PaymentMethod { get; set; }
        
        /// <summary>
        /// The date and time when the order was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// The date and time when the order was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// The list of items in the order
        /// </summary>
        public List<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();
    }
}
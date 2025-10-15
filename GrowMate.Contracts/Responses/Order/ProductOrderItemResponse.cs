using GrowMate.Contracts.Utils;
using System;

namespace GrowMate.Contracts.Responses.Order
{
    /// <summary>
    /// Response model representing a product item in an order
    /// </summary>
    public class ProductOrderItemResponse : OrderItemResponse
    {
        /// <summary>
        /// The identifier of the product
        /// </summary>
        public int ProductId { get; set; }
        
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
        /// The total price from database (snapshot at order time)
        /// </summary>
        public decimal? TotalPriceFromDb { get; set; }
        
        /// <summary>
        /// The total price from database formatted as a VND currency string
        /// </summary>
        public string TotalPriceFromDbFormatted => TotalPriceFromDb.HasValue ? CurrencyUtils.FormatVND(TotalPriceFromDb.Value) : "";
        
        /// <summary>
        /// The type of order item (always "Product")
        /// </summary>
        public override string ItemType => "Product";
    }
}

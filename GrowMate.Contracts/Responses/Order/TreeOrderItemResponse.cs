using GrowMate.Contracts.Utils;
using System;

namespace GrowMate.Contracts.Responses.Order
{
    /// <summary>
    /// Response model representing a tree listing item in an order
    /// </summary>
    public class TreeOrderItemResponse : OrderItemResponse
    {
        /// <summary>
        /// The identifier of the tree listing
        /// </summary>
        public int ListingId { get; set; }
        
        /// <summary>
        /// The product type (e.g., "Sầu riêng", "Nhãn")
        /// </summary>
        public string ProductType { get; set; }
        
        /// <summary>
        /// The product variety (e.g., "Ri6", "Thái")
        /// </summary>
        public string ProductVariety { get; set; }
        
        /// <summary>
        /// The farm name where the tree is located
        /// </summary>
        public string FarmName { get; set; }
        
        /// <summary>
        /// The quantity of trees adopted
        /// </summary>
        public int TreeQuantity { get; set; }
        
        /// <summary>
        /// The unit price per tree at the time of ordering (in VND)
        /// </summary>
        public decimal TreeUnitPrice { get; set; }
        
        /// <summary>
        /// The unit price formatted as a VND currency string
        /// </summary>
        public string TreeUnitPriceFormatted => CurrencyUtils.FormatVND(TreeUnitPrice);

        /// <summary>
        /// Number of years adopted/rented
        /// </summary>
        public int TreeYears { get; set; }
        
        /// <summary>
        /// The total price for this tree order item (tree_quantity * tree_unit_price) in VND
        /// </summary>
        public decimal TreeTotalPrice => TreeQuantity * TreeUnitPrice;
        
        /// <summary>
        /// The total price formatted as a VND currency string
        /// </summary>
        public string TreeTotalPriceFormatted => CurrencyUtils.FormatVND(TreeTotalPrice);
        
        /// <summary>
        /// The total price from database (snapshot at order time)
        /// </summary>
        public decimal? TreeTotalPriceFromDb { get; set; }
        
        /// <summary>
        /// The total price from database formatted as a VND currency string
        /// </summary>
        public string TreeTotalPriceFromDbFormatted => TreeTotalPriceFromDb.HasValue ? CurrencyUtils.FormatVND(TreeTotalPriceFromDb.Value) : "";
        
        /// <summary>
        /// The type of order item (always "Tree")
        /// </summary>
        public override string ItemType => "Tree";
    }
}

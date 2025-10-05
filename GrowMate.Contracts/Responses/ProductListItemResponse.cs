namespace GrowMate.Contracts.Responses
{
    public sealed class ProductListItemResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }

        public string CategoryName { get; set; }
        public string UnitName { get; set; }
        public string ProductTypeName { get; set; }
        public string FarmerName { get; set; }

        public string MainImageUrl { get; set; } // first media if any
    }
}
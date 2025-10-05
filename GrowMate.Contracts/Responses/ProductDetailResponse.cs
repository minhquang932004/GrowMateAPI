namespace GrowMate.Contracts.Responses
{
    public sealed class ProductDetailResponse
    {
        public int ProductId { get; set; }
        public int FarmerId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }

        public string CategoryName { get; set; }
        public string ProductTypeName { get; set; }
        public string UnitName { get; set; }
        public string FarmerName { get; set; }

        public List<MediaResponse> Media { get; set; } = new();
        public string MainImageUrl { get; set; }
    }
}
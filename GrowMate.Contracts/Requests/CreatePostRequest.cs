namespace GrowMate.Contracts.Requests
{
    public class CreatePostRequest
    {

        public int FarmerId { get; set; }

        public string ProductName { get; set; }

        public string ProductType { get; set; }

        public string ProductVariety { get; set; }

        public string FarmName { get; set; }

        public string Origin { get; set; }

        public decimal PricePerYear { get; set; }

        public decimal? HarvestWeight { get; set; }

        public string Unit { get; set; }

        public int? HarvestFrequency { get; set; }

        public int TreeQuantity { get; set; }

        public string Description { get; set; }

        public List<CreateMediaRequest>? CreateMediaPostRequests { get; set; }

    }

}

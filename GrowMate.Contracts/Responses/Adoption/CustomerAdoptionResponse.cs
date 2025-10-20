using System;

namespace GrowMate.Contracts.Responses.Adoption
{
    public class CustomerAdoptionResponse
    {
        public int AdoptionId { get; set; }
        public int TreeId { get; set; }
        public int ListingId { get; set; }
        public int FarmerId { get; set; }
        public string TreeName { get; set; }
        public string FarmerName { get; set; }
        public string UniqueCode { get; set; }
        public string Description { get; set; }
        public string Coordinates { get; set; }
        public string HealthStatus { get; set; }
        public string AvailabilityStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PrimaryImageUrl { get; set; }
        public decimal PricePerYear { get; set; }
        public int Years { get; set; }
        public decimal TotalPrice { get; set; }
        public string PostCode { get; set; }
    }
}

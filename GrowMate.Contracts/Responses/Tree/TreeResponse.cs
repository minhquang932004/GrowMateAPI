using System;

namespace GrowMate.Contracts.Responses.Tree
{
    public class TreeResponse
    {
        public int TreeId { get; set; }
        public int ListingId { get; set; }
        public string UniqueCode { get; set; }
        public string? Description { get; set; }
        public string? Coordinates { get; set; }
        public string? HealthStatus { get; set; }
        public string? AvailabilityStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string TreeName { get; set; }
        public string FarmerName { get; set; }
        public string PrimaryImageUrl { get; set; }
    }

    public class TreeDetailResponse
    {
        public int TreeId { get; set; }
        public int ListingId { get; set; }
        public string UniqueCode { get; set; }
        public string? Description { get; set; }
        public string? Coordinates { get; set; }
        public string? HealthStatus { get; set; }
        public string? AvailabilityStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string TreeName { get; set; }
        public string FarmerName { get; set; }
        public string PrimaryImageUrl { get; set; }
        public decimal PricePerTree { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string ListingStatus { get; set; }
        public List<AdoptionSummaryResponse> Adoptions { get; set; } = new();
    }

    public class AdoptionSummaryResponse
    {
        public int AdoptionId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
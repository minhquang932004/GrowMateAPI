using System.ComponentModel.DataAnnotations;

namespace GrowMate.Contracts.Requests.Tree
{
    public class CreateTreeRequest
    {
        [Required]
        public int ListingId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string UniqueCode { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? Coordinates { get; set; }
        
        [StringLength(50)]
        public string? HealthStatus { get; set; }
        
        [StringLength(50)]
        public string? AvailabilityStatus { get; set; }
    }

    public class UpdateTreeRequest
    {
        [StringLength(50)]
        public string? UniqueCode { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? Coordinates { get; set; }
        
        [StringLength(50)]
        public string? HealthStatus { get; set; }
        
        [StringLength(50)]
        public string? AvailabilityStatus { get; set; }
    }

    public class UpdateTreeStatusRequest
    {
        [Required]
        [StringLength(50)]
        public string HealthStatus { get; set; }
        
        [Required]
        [StringLength(50)]
        public string AvailabilityStatus { get; set; }
    }
}

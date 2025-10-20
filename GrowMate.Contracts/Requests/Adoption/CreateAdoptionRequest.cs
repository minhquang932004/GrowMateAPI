using System.ComponentModel.DataAnnotations;

namespace GrowMate.Contracts.Requests.Adoption
{
    public class CreateAdoptionRequest
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int TreeId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public int? OrderId { get; set; }
        
        public string? PrimaryImageUrl { get; set; }
    }

    public class UpdateAdoptionRequest
    {
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string? PrimaryImageUrl { get; set; }
    }

    public class UpdateAdoptionStatusRequest
    {
        [Required]
        public string Status { get; set; }
    }
}

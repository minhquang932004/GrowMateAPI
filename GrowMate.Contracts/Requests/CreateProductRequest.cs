using System.ComponentModel.DataAnnotations;

namespace GrowMate.Contracts.Requests
{
    public class CreateProductRequest
    {
        [Required]
        public int FarmerId { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductTypeId { get; set; }
        public int? UnitId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Slug { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Stock { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Contracts.Responses
{
    public class TreeListingResponse
    {
        public int ListingId { get; set; }

        public int PostId { get; set; }

        public int FarmerId { get; set; }

        public decimal PricePerTree { get; set; }

        public int TotalQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public string Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<TreeResponse>? TreeResponses { get; set; }
    }
}

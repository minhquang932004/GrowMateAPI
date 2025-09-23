using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Contracts.Requests
{
    public class TreeListingRequest
    {
        public int PostId { get; set; }

        public int FarmerId { get; set; }

        public decimal PricePerTree { get; set; }

        public int TotalQuantity { get; set; }

        public int AvailableQuantity { get; set; }
    }
}

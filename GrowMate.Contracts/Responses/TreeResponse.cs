using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Contracts.Responses
{
    public class TreeResponse
    {
        public int TreeId { get; set; }

        public int ListingId { get; set; }

        public string UniqueCode { get; set; }

        public string Description { get; set; }

        public string Coordinates { get; set; }

        public string HealthStatus { get; set; }

        public string AvailabilityStatus { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}

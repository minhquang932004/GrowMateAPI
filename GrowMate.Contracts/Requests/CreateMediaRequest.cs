using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Contracts.Requests
{
    public class CreateMediaRequest
    {
        public string MediaUrl { get; set; }
        public string MediaType { get; set; }
    }
}

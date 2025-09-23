using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Repositories.Models.Statuses
{
    public static class TreeListingStatuses
    {
        // Match current usage in code. DB default is "draft".
        public const string Active = "Active";
        public const string SoldOut = "Sold_Out";
        public const string Removed = "Removed";
    }
}

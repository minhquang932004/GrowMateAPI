using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrowMate.Contracts.Requests.Media;

namespace GrowMate.Contracts.Requests.MonthlyReport
{
    /// <summary>
    /// Request model for updating a monthly report
    /// </summary>
    public class UpdateMonthlyReportRequest
    {
        /// <summary>
        /// Activities performed during this month
        /// </summary>
        [Required]
        public string Activities { get; set; }

        /// <summary>
        /// Additional notes for this report
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Status of the report (draft, published)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// List of media items (images/videos) for this report - replaces existing media
        /// </summary>
        public List<MediaItemRequest>? Media { get; set; }
    }
}


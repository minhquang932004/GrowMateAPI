using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrowMate.Contracts.Requests.Media;

namespace GrowMate.Contracts.Requests.MonthlyReport
{
    /// <summary>
    /// Request model for creating a new monthly report
    /// </summary>
    public class CreateMonthlyReportRequest
    {
        /// <summary>
        /// The identifier of the adoption this report belongs to
        /// </summary>
        [Required]
        public int AdoptionId { get; set; }

        /// <summary>
        /// The identifier of the farmer creating this report
        /// </summary>
        [Required]
        public int FarmerId { get; set; }

        /// <summary>
        /// The month of the report (1-12)
        /// </summary>
        [Required]
        [Range(1, 12)]
        public int ReportMonth { get; set; }

        /// <summary>
        /// The year of the report
        /// </summary>
        [Required]
        public int ReportYear { get; set; }

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
        /// List of media items (images/videos) for this report
        /// </summary>
        public List<MediaItemRequest>? Media { get; set; }
    }
}


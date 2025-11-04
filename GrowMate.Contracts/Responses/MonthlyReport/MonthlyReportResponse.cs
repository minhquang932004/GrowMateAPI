using System;
using System.Collections.Generic;
using GrowMate.Contracts.Responses.Media;

namespace GrowMate.Contracts.Responses.MonthlyReport
{
    /// <summary>
    /// Response model for a monthly report
    /// </summary>
    public class MonthlyReportResponse
    {
        /// <summary>
        /// The unique identifier for the report
        /// </summary>
        public int ReportId { get; set; }

        /// <summary>
        /// The identifier of the adoption this report belongs to
        /// </summary>
        public int AdoptionId { get; set; }

        /// <summary>
        /// The identifier of the farmer who created this report
        /// </summary>
        public int FarmerId { get; set; }

        /// <summary>
        /// The month of the report (1-12)
        /// </summary>
        public int ReportMonth { get; set; }

        /// <summary>
        /// The year of the report
        /// </summary>
        public int ReportYear { get; set; }

        /// <summary>
        /// Activities performed during this month
        /// </summary>
        public string Activities { get; set; }

        /// <summary>
        /// Additional notes for this report
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Status of the report (draft, published)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The date and time when the report was created
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the report was published
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// List of media items (images/videos) associated with this report
        /// </summary>
        public List<MediaReportResponse>? Media { get; set; }
    }
}


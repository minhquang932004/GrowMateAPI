using System;

namespace GrowMate.Contracts.Responses.MonthlyReport
{
    /// <summary>
    /// Response model for media items associated with a monthly report
    /// </summary>
    public class MediaReportResponse
    {
        /// <summary>
        /// The unique identifier for the media item
        /// </summary>
        public int MediaId { get; set; }

        /// <summary>
        /// The identifier of the report this media belongs to
        /// </summary>
        public int? ReportId { get; set; }

        /// <summary>
        /// The URL where the media is stored/accessed
        /// </summary>
        public string MediaUrl { get; set; }

        /// <summary>
        /// The type of media (Image, Video)
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Whether this is the primary image
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// The date and time when the media was created
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the media was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}


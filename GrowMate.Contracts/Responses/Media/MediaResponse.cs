namespace GrowMate.Contracts.Responses.Media
{
    /// <summary>
    /// Response model representing media items (images/videos)
    /// Note: This is named "Media" to match the database name
    /// </summary>
    public class MediaResponse
    {
        /// <summary>
        /// The unique identifier for the media item
        /// </summary>
        public int MediaId { get; set; }
        
        /// <summary>
        /// The URL where the media is stored/accessed
        /// </summary>
        public string MediaUrl { get; set; }
        
        /// <summary>
        /// The type of media (image, video, etc.)
        /// </summary>
        public string MediaType { get; set; }
    }
}
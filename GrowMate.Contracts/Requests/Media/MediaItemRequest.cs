namespace GrowMate.Contracts.Requests.Media
{
    /// <summary>
    /// Request model for media items (images/videos) to be attached to products or posts
    /// Note: This is named with "Media" to match the database (not "Medium")
    /// </summary>
    public class MediaItemRequest
    {
        /// <summary>
        /// The URL where the media is stored/accessed
        /// </summary>
        public string MediaUrl { get; set; }
        
        /// <summary>
        /// The type of media (e.g., "Image", "Video")
        /// Should match your MediaType enum
        /// </summary>
        public string MediaType { get; set; }
    }
}
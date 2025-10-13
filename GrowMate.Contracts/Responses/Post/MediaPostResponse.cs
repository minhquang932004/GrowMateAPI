using System;

namespace GrowMate.Contracts.Responses.Post
{
    /// <summary>
    /// Response model representing media (images/videos) associated with a post
    /// Note: This is named MediaPostResponse to match your code, but in the database it's "Media"
    /// </summary>
    public class MediaPostResponse
    {
        /// <summary>
        /// The unique identifier for the media item
        /// </summary>
        public int MediaId { get; set; }

        /// <summary>
        /// The identifier of the post this media belongs to
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// The URL where the media is stored/accessed
        /// </summary>
        public string MediaUrl { get; set; }

        /// <summary>
        /// The type of media (image, video, etc.)
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// The date and time when the media was added
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the media was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
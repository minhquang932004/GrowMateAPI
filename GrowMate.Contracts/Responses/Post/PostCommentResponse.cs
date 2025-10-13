using System;

namespace GrowMate.Contracts.Responses.Post
{
    /// <summary>
    /// Response model representing a comment on a post
    /// </summary>
    public class PostCommentResponse
    {
        /// <summary>
        /// The unique identifier for the comment
        /// </summary>
        public int CommentId { get; set; }

        /// <summary>
        /// The identifier of the post this comment belongs to
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// The identifier of the user who made the comment
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The content of the comment
        /// </summary>
        public string CommentContent { get; set; }

        /// <summary>
        /// The date and time when the comment was created
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the comment was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
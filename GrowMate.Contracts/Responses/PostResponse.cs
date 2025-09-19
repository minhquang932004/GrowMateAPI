namespace GrowMate.Contracts.Responses
{
    public class PostResponse
    {
        public int PostId { get; set; }

        public int FarmerId { get; set; }

        public string ProductName { get; set; }

        public string ProductType { get; set; }

        public string ProductVariety { get; set; }

        public string FarmName { get; set; }

        public string Origin { get; set; }

        public decimal PricePerYear { get; set; }

        public decimal? HarvestWeight { get; set; }

        public string Unit { get; set; }

        public int? HarvestFrequency { get; set; }

        public int TreeQuantity { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        public List<MediaPostListResponse>? MediaPostList { get; set; }
        public List<PostCommentListResponse>? PostCommentList { get; set;} 
    }

    public class MediaPostListResponse
    {
        public int MediaId { get; set; }

        public int PostId { get; set; }

        public string MediaUrl { get; set; }

        public string MediaType { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class PostCommentListResponse
    {
        public int CommentId { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        public string CommentContent { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

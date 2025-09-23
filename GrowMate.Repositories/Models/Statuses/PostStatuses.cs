namespace GrowMate.Repositories.Models.Statuses
{
    public static class PostStatuses
    {
        // Match current usage in code. DB default is "draft".
        public const string Pending = "PENDING";
        public const string Canceled = "CANCELED";
        public const string Draft = "DRAFT";
        // Add more as you formalize the workflow:
        public const string Approved = "APPROVED";
        // public const string Active = "ACTIVE";
    }
}
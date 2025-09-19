namespace GrowMate.Repositories.Models
{
    public class EmailVerification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CodeHash { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }

        // Navigation
        public virtual User User { get; set; } = null!;
    }
}

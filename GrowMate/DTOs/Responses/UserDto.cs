namespace GrowMate.DTOs.Responses
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public int Role { get; set; }
        public string RoleName { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
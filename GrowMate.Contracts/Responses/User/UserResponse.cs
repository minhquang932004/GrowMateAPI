using GrowMate.Contracts.Responses.Customer;
using GrowMate.Contracts.Responses.Farmer;

namespace GrowMate.Contracts.Responses.User
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public int Role { get; set; }
        public string RoleName { get; set; } = null!;
        public string? ProfileImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public CustomerResponse? Customer { get; set; }
        public FarmerResponse? Farmer { get; set; }
    }
}
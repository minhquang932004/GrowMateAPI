namespace GrowMate.DTOs.Requests
{
    public class CreateUserByAdminRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public int Role { get; set; }
        public string? ProfileImageUrl { get; set; }

        public CustomerRequest? CustomerRequest { get; set; }
        public FarmerRequest? FarmerRequest { get; set; }
    }

    public class CustomerRequest
    {
        public string? ShippingAddress { get; set; }
        public decimal? WalletBalance { get; set; }
    }

    public class FarmerRequest
    {
        public string FarmName { get; set; }

        public string FarmAddress { get; set; }

        public string ContactPhone { get; set; }

        public string VerificationStatus { get; set; }

    }
}

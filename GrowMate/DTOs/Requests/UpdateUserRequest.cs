namespace GrowMate.DTOs.Requests
{
    public class UpdateUserRequest
    {
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }

        public UpdateCustomerRequest? UpdateCustomer { get; set; }
        public UpdateFarmerRequest? UpdateFarmer { get; set; }
    }

    public class UpdateCustomerRequest
    {
        public string? ShippingAddress { get; set; }
        public decimal? WalletBalance { get; set; }
    }

    public class UpdateFarmerRequest
    {
        public string FarmName { get; set; }

        public string FarmAddress { get; set; }

        public string ContactPhone { get; set; }

        public string VerificationStatus { get; set; }

    }
}

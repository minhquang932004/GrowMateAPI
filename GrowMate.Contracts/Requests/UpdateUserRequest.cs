namespace GrowMate.Contracts.Requests
{
    public class UpdateUserRequest
    {
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }

        public CustomerRequest? UpdateCustomer { get; set; }
        public FarmerRequest? UpdateFarmer { get; set; }
    }

}

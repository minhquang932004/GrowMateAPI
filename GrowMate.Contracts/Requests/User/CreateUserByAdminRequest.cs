using GrowMate.Contracts.Requests.Customer;
using GrowMate.Contracts.Requests.Farmer;

namespace GrowMate.Contracts.Requests.User
{
    /// <summary>
    /// Request model for creating a user account by an administrator
    /// </summary>
    public class CreateUserByAdminRequest
    {
        /// <summary>
        /// Email address for the new user account
        /// </summary>
        public string Email { get; set; } = null!;
        
        /// <summary>
        /// Password for the new user account
        /// </summary>
        public string Password { get; set; } = null!;
        
        /// <summary>
        /// Full name of the user
        /// </summary>
        public string FullName { get; set; } = null!;
        
        /// <summary>
        /// Optional phone number for the user
        /// </summary>
        public string? Phone { get; set; }
        
        /// <summary>
        /// Role identifier for the user (e.g., 1 = Admin, 2 = Customer, 3 = Farmer)
        /// </summary>
        public int Role { get; set; }
        
        /// <summary>
        /// Optional profile image URL for the user
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Customer-specific details if the user is a customer
        /// </summary>
        public CustomerRequest? CustomerRequest { get; set; }
        
        /// <summary>
        /// Farmer-specific details if the user is a farmer
        /// </summary>
        public FarmerRequest? FarmerRequest { get; set; }
    }
}
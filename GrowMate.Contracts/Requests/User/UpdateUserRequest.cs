using GrowMate.Contracts.Requests.Customer;
using GrowMate.Contracts.Requests.Farmer;

namespace GrowMate.Contracts.Requests.User
{
    /// <summary>
    /// Request model for updating a user's profile information
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// Email address of the user (may be used for verification)
        /// </summary>
        public string Email { get; set; } = null!;
        
        /// <summary>
        /// Updated full name for the user
        /// </summary>
        public string FullName { get; set; } = null!;
        
        /// <summary>
        /// Updated phone number for the user
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Customer-specific information to update (if applicable)
        /// </summary>
        public CustomerRequest? UpdateCustomer { get; set; }
        
        /// <summary>
        /// Farmer-specific information to update (if applicable)
        /// </summary>
        public FarmerRequest? UpdateFarmer { get; set; }
    }
}
namespace GrowMate.Contracts.Requests.User
{
    /// <summary>
    /// Request model for updating a user's information by an administrator
    /// Extends the standard UpdateUserRequest with admin-specific fields
    /// </summary>
    public class UpdateUserByAdminRequest : UpdateUserRequest
    {
        /// <summary>
        /// The role to assign to the user (e.g., 1 = Admin, 2 = Customer, 3 = Farmer)
        /// </summary>
        public int Role { get; set; }
        
        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
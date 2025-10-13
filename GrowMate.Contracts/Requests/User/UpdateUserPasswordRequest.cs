namespace GrowMate.Contracts.Requests.User
{
    /// <summary>
    /// Request model for updating a user's password
    /// </summary>
    public class UpdateUserPasswordRequest
    {
        /// <summary>
        /// The user's current password for verification
        /// </summary>
        public string OldPassword { get; set; }
        
        /// <summary>
        /// The new password to set for the user account
        /// </summary>
        public string NewPassword { get; set; }
    }
}
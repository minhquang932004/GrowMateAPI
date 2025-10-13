namespace GrowMate.Contracts.Requests.Auth
{
    /// <summary>
    /// Request model for completing the password reset process
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// Email address of the account for which the password is being reset
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// The verification code sent to the user's email
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// The new password to set for the account
        /// </summary>
        public string NewPassword { get; set; }
    }
}
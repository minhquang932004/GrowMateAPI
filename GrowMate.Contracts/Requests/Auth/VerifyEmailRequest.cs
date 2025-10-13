namespace GrowMate.Contracts.Requests.Auth
{
    /// <summary>
    /// Request model for verifying a user's email address
    /// </summary>
    public class VerifyEmailRequest
    {
        /// <summary>
        /// Email address to verify
        /// </summary>
        public string Email { get; set; } = null!;
        
        /// <summary>
        /// Verification code sent to the user's email
        /// </summary>
        public string Code { get; set; } = null!;
    }
}
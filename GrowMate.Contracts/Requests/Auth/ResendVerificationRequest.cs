namespace GrowMate.Contracts.Requests.Auth
{
    /// <summary>
    /// Request model for resending an email verification code
    /// </summary>
    public class ResendVerificationRequest
    {
        /// <summary>
        /// Email address for which to resend the verification code
        /// </summary>
        public string Email { get; set; }
    }
}
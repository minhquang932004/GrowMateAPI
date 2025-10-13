namespace GrowMate.Contracts.Requests.Auth
{
    /// <summary>
    /// Request model for initiating password reset process
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// Email address of the account for which to reset the password
        /// </summary>
        public string Email { get; set; }
    }
}
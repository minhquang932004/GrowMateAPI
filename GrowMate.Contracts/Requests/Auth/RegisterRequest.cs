namespace GrowMate.Contracts.Requests.Auth
{
    /// <summary>
    /// Request model for user registration
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Full name of the user registering
        /// </summary>
        public string FullName { get; set; } = null!;
        
        /// <summary>
        /// Email address for the new account
        /// </summary>
        public string Email { get; set; } = null!;
        
        /// <summary>
        /// Password for the new account
        /// </summary>
        public string Password { get; set; } = null!;
    }
}
namespace GrowMate.Contracts.Requests.Auth
{
    /// <summary>
    /// Request model for user login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// User's email address for authentication
        /// </summary>
        public string Email { get; set; } = null!;
        
        /// <summary>
        /// User's password for authentication
        /// </summary>
        public string Password { get; set; } = null!;
    }
}
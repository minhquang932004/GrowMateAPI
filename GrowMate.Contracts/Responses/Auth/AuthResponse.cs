namespace GrowMate.Contracts.Responses.Auth
{
    /// <summary>
    /// Response model for general authentication operations
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Message describing the result of the authentication operation
        /// </summary>
        public string Message { get; set; } = null!;
        
        /// <summary>
        /// Indicates whether the authentication operation was successful
        /// </summary>
        public bool Success { get; set; }
    }
}
using GrowMate.Contracts.Responses.User;
using System;

namespace GrowMate.Contracts.Responses.Auth
{
    /// <summary>
    /// Response model for user login operations
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Indicates whether the login attempt was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message describing the result of the login operation
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// JWT token for authenticated API access
        /// </summary>
        public string? Token { get; set; }
        
        /// <summary>
        /// Date and time when the token expires
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// User details for the authenticated user
        /// </summary>
        public UserResponse? User { get; set; }

        /// <summary>
        /// Optional error code for troubleshooting (e.g., "AUTH_INVALID_CREDENTIALS")
        /// </summary>
        public string? ErrorCode { get; set; }
        
        /// <summary>
        /// GUID to correlate this response with server logs
        /// </summary>
        public string? CorrelationId { get; set; }
    }
}
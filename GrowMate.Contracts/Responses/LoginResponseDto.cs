namespace GrowMate.Contracts.Responses
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public UserDto? User { get; set; }
        public CustomerDto? Customer { get; set; }

        // Optional diagnostics fields (do not reveal sensitive info)
        public string? ErrorCode { get; set; }       // e.g., "AUTH_INVALID_CREDENTIALS"
        public string? CorrelationId { get; set; }   // GUID to match with logs
    }
}
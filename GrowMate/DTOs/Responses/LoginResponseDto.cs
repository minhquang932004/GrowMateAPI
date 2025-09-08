namespace GrowMate.DTOs.Responses
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public UserDto? User { get; set; }
        public CustomerDto? Customer { get; set; }
    }
}
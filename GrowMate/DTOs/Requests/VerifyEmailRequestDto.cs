namespace GrowMate.DTOs.Requests
{
    public class VerifyEmailRequestDto
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}

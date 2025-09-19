namespace GrowMate.Contracts.Requests
{
    public class RegisterRequestDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

    }
}

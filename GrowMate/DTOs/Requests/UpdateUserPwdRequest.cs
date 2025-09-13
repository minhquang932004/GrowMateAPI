namespace GrowMate.DTOs.Requests
{
    public class UpdateUserPwdRequest
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
    }
}

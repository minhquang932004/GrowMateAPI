namespace GrowMate.Contracts.Requests
{
    public class UpdateUserByAdminRequest : UpdateUserRequest
    {
        public int Role {  get; set; }
        public bool? IsActive { get; set; }
    }
}

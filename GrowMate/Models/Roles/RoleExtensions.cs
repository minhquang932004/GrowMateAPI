namespace GrowMate.Models.Roles
{
    public static class RoleExtensions
    {
        public static bool IsCustomer(this User user) => user.Role == UserRoles.Customer;
        public static bool IsFarmer(this User user) => user.Role == UserRoles.Farmer;
        public static bool IsAdmin(this User user) => user.Role == UserRoles.Admin;
    }
}

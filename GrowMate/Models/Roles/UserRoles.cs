namespace GrowMate.Models.Roles
{
    public static class UserRoles
    {
        // Numeric values match the database
        public const int Guest = 0;
        public const int Customer = 1;
        public const int Farmer = 2;
        public const int Admin = 3;

        public static string ToName(int role) => role switch
        {
            Customer => "Customer",
            Farmer => "Farmer",
            Admin => "Admin",
            _ => "Guest"
        };
    }

    // Optional enum for readability in code (still stored as int in DB)
    public enum UserRole
    {
        Customer = UserRoles.Customer,
        Farmer = UserRoles.Farmer,
        Admin = UserRoles.Admin,
        Guest = UserRoles.Guest
    }
}

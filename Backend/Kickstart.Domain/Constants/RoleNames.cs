namespace Kickstart.Domain.Constants
{
    /// <summary>
    /// System role names with special logic in the application.
    /// Add only roles that have hardcoded behavior (e.g. SuperAdmin tenant bypass).
    /// Custom roles created dynamically are stored in the database.
    /// </summary>
    public static class RoleNames
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Manager = "Manager";
    }
}

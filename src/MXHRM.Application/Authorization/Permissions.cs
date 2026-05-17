namespace MXHRM.Application.Authorization;

public static class Permissions
{
    public static class Employee
    {
        public const string Read = "employee.read";
        public const string Create = "employee.create";
        public const string Update = "employee.update";
        public const string Delete = "employee.delete";
    }

    public static class Role
    {
        public const string Manage = "role.manage";
    }

    public static class Audit
    {
        public const string Read = "audit.read";
    }

    public static class Activity
    {
        public const string Read = "activity.read";
    }
}

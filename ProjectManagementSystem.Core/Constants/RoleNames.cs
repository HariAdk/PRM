namespace ProjectManagementSystem.Core.Constants;

/// <summary>JWT role claim values — must match <see cref="Enums.UserRole"/> names.</summary>
public static class RoleNames
{
    public const string Admin = nameof(Enums.UserRole.Admin);
    public const string Manager = nameof(Enums.UserRole.Manager);
    public const string Employee = nameof(Enums.UserRole.Employee);
}

namespace ProjectManagementSystem.Core.Helpers;

public static class PasswordExpiryHelper
{
    public const int PasswordValidityMonths = 3;

    /// <summary>Admin-assigned temporary password — user must change on next login.</summary>
    public static DateTime ImmediateChangeRequired => DateTime.UtcNow;

    /// <summary>Password chosen by the user — valid for three months.</summary>
    public static DateTime ValidUntilAfterUserChange() =>
        DateTime.UtcNow.AddMonths(PasswordValidityMonths);

    public static bool RequiresChange(DateTime passwordExpiresAt) =>
        DateTime.UtcNow >= passwordExpiresAt;
}

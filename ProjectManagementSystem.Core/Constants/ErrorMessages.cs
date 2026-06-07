namespace ProjectManagementSystem.Core.Constants;

public static class ErrorMessages
{
    public const string InvalidCredentials = "Invalid username or password.";
    public const string AccountDeactivated = "This account has been deactivated.";
    public const string AdminSignUpForbidden = "Admin accounts cannot be created via Sign Up.";
    public const string DuplicateUsernameOrEmail = "Username or email is already in use.";
    public const string PasswordsDoNotMatch = "Passwords do not match.";
    public const string InvalidUserIdentity = "Invalid or missing user identity.";
    public const string CannotChangeOtherUserPassword = "You can only change your own password.";
    public const string NoEmployeeProfile = "No active employee profile linked to your account.";
    public const string SystemConfigNotFound = "System configuration not found.";
    public const string TimesheetAlreadySubmitted = "A timesheet for this week has already been submitted.";
    public const string TimesheetNotFound = "Timesheet not found.";
    public const string InvalidWeekStartDate = "Invalid week start date format. Use yyyy-MM-dd.";
    public const string AllocationAlreadyEnded = "This allocation has already ended.";
    public const string OnlyEmployeesCanBeAllocated =
        "Only individual contributors (Employee role) can be allocated to projects.";
}

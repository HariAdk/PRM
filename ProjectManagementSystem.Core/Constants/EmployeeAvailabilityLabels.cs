namespace ProjectManagementSystem.Core.Constants;

public static class EmployeeAvailabilityLabels
{
    public const string OverAllocated = "OVER";
    public const string FullyAllocated = "FULL";
    public const string Bench = "BENCH";
    public const string AllocatedFull = "ALLOCATED (100%)";
    public const string NotApplicable = "N/A";

    public static string ProfileStatus(string userRole, string employeeStatus) =>
        userRole.Equals(RoleNames.Manager, StringComparison.OrdinalIgnoreCase)
            ? NotApplicable
            : employeeStatus.Replace(" ", "_").ToUpperInvariant();

    public static string PartialAvailability(int freePercent) => $"{freePercent}% free";

    public static string AllocatedPartial(int percent) => $"ALLOCATED ({percent}%)";
}

using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Core.Helpers;

public static class MilestoneStatusHelper
{
    public static bool IsDone(string status) =>
        string.Equals(status, nameof(MilestoneStatus.Done), StringComparison.OrdinalIgnoreCase);

    public static bool IsOverdue(string status, DateOnly dueDate, DateOnly today) =>
        !IsDone(status) && dueDate < today;
}

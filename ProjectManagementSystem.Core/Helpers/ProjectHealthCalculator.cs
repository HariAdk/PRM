using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Core.Helpers;

/// <summary>Computes live project health for manager views (SRP — separate from persistence scheduler).</summary>
public static class ProjectHealthCalculator
{
    public static string ComputeDisplayHealth(
        ProjectDto project,
        IReadOnlyList<MilestoneDto> milestones,
        IReadOnlyList<AllocationDto> allocations,
        IReadOnlyList<TimesheetDto> weekTimesheets,
        int maxWeeklyHours)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var overdueCount = milestones.Count(m => MilestoneStatusHelper.IsOverdue(m.Status, m.DueDate, today));

        var negativeCount = overdueCount;
        var hasLowEffort = false;

        foreach (var alloc in allocations.Where(a => a.IsActive))
        {
            var expected = Math.Round(alloc.UtilisationPercent * maxWeeklyHours / 100m, 0);
            if (expected <= 0) continue;

            var logged = weekTimesheets
                .Where(t => t.EmployeeId == alloc.EmployeeId)
                .SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == project.Id)
                .Sum(e => e.Hours);

            if (logged < expected)
            {
                negativeCount++;
                hasLowEffort = true;
            }
        }

        if (negativeCount >= 2 || (overdueCount > 0 && hasLowEffort))
            return nameof(ProjectHealth.AtRisk);

        if (negativeCount >= 1 || overdueCount > 0)
            return nameof(ProjectHealth.Attention);

        return project.HealthStatus;
    }
}

using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

public class SchedulerService(
    IEmployeeRepository employeeRepo,
    IAllocationRepository allocationRepo,
    IProjectRepository projectRepo,
    ITimesheetRepository timesheetRepo,
    INotificationService notificationService,
    ILogger<SchedulerService> logger) : ISchedulerService
{
    public async Task RunScheduledTasksAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Scheduler run started at {Time}", DateTimeOffset.UtcNow);

        await DeactivateExpiredAllocationsAsync(cancellationToken);
        await RecomputeEmployeeStatusesAsync(cancellationToken);
        await UpdateProjectHealthAsync(cancellationToken);
        await notificationService.ProcessTimesheetEscalationsAsync(cancellationToken);
        await notificationService.ProcessAtRiskProjectNotificationsAsync(cancellationToken);
        await MarkMissedTimesheetsAsync(cancellationToken);

        logger.LogInformation("Scheduler run completed at {Time}", DateTimeOffset.UtcNow);
    }

    private async Task DeactivateExpiredAllocationsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var deactivated = await allocationRepo.DeactivateExpiredAsync(today);

        if (deactivated > 0)
        {
            logger.LogInformation(
                "Deactivated {Count} allocation(s) whose end date passed before {Date}",
                deactivated, today);
        }
    }

    private async Task RecomputeEmployeeStatusesAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var employees = (await employeeRepo.GetAllocatableResourcesAsync()).ToList();
        var allocations = (await allocationRepo.GetAllActiveAsync()).ToList();

        var allocatedToday = allocations
            .Where(a => a.FromDate <= today && a.ToDate >= today)
            .Select(a => a.EmployeeId)
            .ToHashSet();

        foreach (var employee in employees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var newStatus = allocatedToday.Contains(employee.Id)
                ? EmployeeStatus.Allocated
                : EmployeeStatus.Bench;

            if (!string.Equals(employee.Status, newStatus.ToString(), StringComparison.OrdinalIgnoreCase))
                await employeeRepo.SetStatusAsync(employee.Id, newStatus);
        }

        logger.LogInformation("Employee statuses recomputed for {Count} employees", employees.Count);
    }

    private async Task UpdateProjectHealthAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var horizon = today.AddDays(7);
        var activeProjects = (await projectRepo.GetActiveAsync()).ToList();

        foreach (var project in activeProjects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var milestones = (await projectRepo.GetMilestonesAsync(project.Id)).ToList();

            var hasOverdueInProgress = milestones.Any(m =>
                string.Equals(m.Status, nameof(MilestoneStatus.InProgress), StringComparison.OrdinalIgnoreCase) &&
                m.DueDate < today);

            var hasUpcomingNotStarted = milestones.Any(m =>
                string.Equals(m.Status, nameof(MilestoneStatus.NotStarted), StringComparison.OrdinalIgnoreCase) &&
                m.DueDate >= today && m.DueDate <= horizon);

            var hasOverdueAny = milestones.Any(m =>
                MilestoneStatusHelper.IsOverdue(m.Status, m.DueDate, today));

            var health = hasOverdueInProgress || hasOverdueAny
                ? ProjectHealth.AtRisk
                : hasUpcomingNotStarted
                    ? ProjectHealth.Attention
                    : ProjectHealth.OnTrack;

            await projectRepo.UpdateHealthStatusAsync(project.Id, health);
        }

        logger.LogInformation("Project health updated for {Count} active projects", activeProjects.Count);
    }

    private async Task MarkMissedTimesheetsAsync(CancellationToken cancellationToken)
    {
        var lastWeekMonday = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
        var lastWeekSunday = DateOnly.FromDateTime(lastWeekMonday.AddDays(6));
        var weekStart = DateOnly.FromDateTime(lastWeekMonday);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var workingDays = WorkingDayHelper.CountWorkingDaysSinceWeekEnd(lastWeekSunday, today);

        if (workingDays <= SystemDefaults.TimesheetReminderDays)
            return;

        var employeeIds = (await allocationRepo.GetEmployeeIdsAllocatedBetweenAsync(
            weekStart, lastWeekSunday)).ToList();

        var missedCount = 0;
        foreach (var employeeId in employeeIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await timesheetRepo.ExistsForEmployeeWeekAsync(employeeId, lastWeekMonday))
                continue;

            await timesheetRepo.CreateMissedAsync(employeeId, lastWeekMonday);
            missedCount++;
        }

        logger.LogInformation(
            "Missed timesheet check complete for week {WeekStart}: {MissedCount} record(s) created",
            weekStart, missedCount);
    }
}

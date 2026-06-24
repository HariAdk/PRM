using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Application.Email;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Notification;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

/// <summary>
/// Sends all system emails. Called exclusively from <see cref="SchedulerService"/> (background scheduler).
/// </summary>
public class NotificationService(
    IAllocationRepository allocationRepo,
    ITimesheetRepository timesheetRepo,
    IEmployeeRepository employeeRepo,
    IUserRepository userRepo,
    IProjectRepository projectRepo,
    ITimesheetReminderRepository reminderRepo,
    IAiService aiService,
    IEmailService emailService,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task ProcessTimesheetEscalationsAsync(CancellationToken cancellationToken = default)
    {
        if (!WorkingDayHelper.IsWorkingDay(DateTime.Today))
            return;

        var lastWeekMonday = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
        var lastWeekSunday = DateOnly.FromDateTime(lastWeekMonday.AddDays(6));
        var weekStart = DateOnly.FromDateTime(lastWeekMonday);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var workingDaysSinceDeadline = WorkingDayHelper.CountWorkingDaysSinceWeekEnd(lastWeekSunday, today);

        var employeeIds = (await allocationRepo.GetEmployeeIdsAllocatedBetweenAsync(weekStart, lastWeekSunday))
            .ToList();

        foreach (var employeeId in employeeIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await timesheetRepo.HasSubmittedForWeekAsync(employeeId, lastWeekMonday))
                continue;

            var state = await reminderRepo.GetOrCreateAsync(employeeId, weekStart);

            if (state.RestoredAt.HasValue)
                continue;

            if (state.IsFrozen)
                continue;

            if (workingDaysSinceDeadline >= 1 &&
                workingDaysSinceDeadline <= SystemDefaults.TimesheetReminderDays &&
                state.ReminderCount < workingDaysSinceDeadline &&
                state.LastReminderDate != today)
            {
                state = state with
                {
                    ReminderCount = workingDaysSinceDeadline,
                    LastReminderDate = today
                };
                await reminderRepo.UpdateAsync(state);

                var employee = await employeeRepo.GetByIdAsync(employeeId);
                if (employee is not null && !string.IsNullOrWhiteSpace(employee.Email))
                {
                    await emailService.SendAsync(
                        employee.Email,
                        $"[PRM] Timesheet Reminder {workingDaysSinceDeadline}/{SystemDefaults.TimesheetReminderDays}",
                        EmailTemplates.TimesheetReminder(
                            employee.FullName,
                            weekStart,
                            workingDaysSinceDeadline,
                            SystemDefaults.TimesheetReminderDays));
                }

                logger.LogInformation(
                    "Timesheet reminder {Day} emailed for employee {EmployeeId}, week {Week}",
                    workingDaysSinceDeadline, employeeId, weekStart);
                continue;
            }

            if (workingDaysSinceDeadline > SystemDefaults.TimesheetReminderDays &&
                state.ReminderCount >= SystemDefaults.TimesheetReminderDays &&
                state.FreezeNotifiedAt is null)
            {
                await FreezeAndNotifyAsync(employeeId, weekStart, lastWeekMonday, state);
            }
        }
    }

    public async Task ProcessAtRiskProjectNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await projectRepo.GetAtRiskProjectsPendingNotificationAsync();

        foreach (var project in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(project.ManagerEmail))
            {
                logger.LogWarning("Skipping at-risk email for project {ProjectId}: manager email missing.", project.ProjectId);
                await projectRepo.MarkAtRiskNotifiedAsync(project.ProjectId);
                continue;
            }

            var riskSummary = await aiService.GetRiskSummaryAsync(
                new AIRiskSummaryRequestDto { ProjectId = project.ProjectId });

            var skillRequirement =
                $"Resources to help project {project.ProjectName} address risks: overdue milestones and under-logged hours";
            var skillMatch = await aiService.GetSkillMatchAsync(
                new AISkillMatchRequestDto { Requirement = skillRequirement });

            var body = EmailTemplates.ProjectAtRisk(
                project,
                EmailTemplates.HealthLabel(project.HealthStatus),
                riskSummary.Summary,
                skillMatch.Matches);

            await emailService.SendAsync(
                project.ManagerEmail,
                $"[PRM] Project At Risk: {project.ProjectName}",
                body);

            await projectRepo.MarkAtRiskNotifiedAsync(project.ProjectId);
            logger.LogInformation("At-risk notification sent for project {ProjectId}.", project.ProjectId);
        }
    }

    private async Task FreezeAndNotifyAsync(
        int employeeId,
        DateOnly weekStart,
        DateTime lastWeekMonday,
        TimesheetReminderStateDto state)
    {
        var employee = await employeeRepo.GetByIdAsync(employeeId);
        if (employee is null)
            return;

        var frozenState = state with
        {
            IsFrozen = true,
            FreezeNotifiedAt = DateTime.UtcNow
        };
        await reminderRepo.UpdateAsync(frozenState);

        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            await emailService.SendAsync(
                employee.Email,
                "[PRM] Timesheet Submission Frozen",
                EmailTemplates.TimesheetFreezeEmployee(employee.FullName, weekStart));
        }

        if (employee.ManagerId.HasValue)
        {
            var manager = await userRepo.GetByIdAsync(employee.ManagerId.Value);
            if (manager is not null && !string.IsNullOrWhiteSpace(manager.Email))
            {
                await emailService.SendAsync(
                    manager.Email,
                    $"[PRM] Timesheet Frozen: {employee.FullName}",
                    EmailTemplates.TimesheetFreezeManager(employee.FullName, weekStart));
            }
        }

        if (!await timesheetRepo.ExistsForEmployeeWeekAsync(employeeId, lastWeekMonday))
            await timesheetRepo.CreateMissedAsync(employeeId, lastWeekMonday);

        logger.LogInformation(
            "Timesheet frozen and notifications sent for employee {EmployeeId}, week {Week}",
            employeeId, weekStart);
    }
}

using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Validation;

namespace ProjectManagementSystem.Application;

public class EmployeePortalService(
    IEmployeeRepository employeeRepo,
    IAllocationRepository allocationRepo,
    ITimesheetRepository timesheetRepo,
    ISystemConfigRepository configRepo) : IEmployeePortalService
{
    public async Task<EmployeeProfileDto> GetProfileAsync(int userId)
    {
        var employee = await RequireEmployeeAsync(userId);
        var allocations = (await allocationRepo.GetByEmployeeIdAsync(employee.Id))
            .OrderByDescending(a => a.IsActive)
            .ThenBy(a => a.ProjectName)
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var activeTotal = allocations
            .Where(a => a.IsActive && a.FromDate <= today && a.ToDate >= today)
            .Sum(a => a.UtilisationPercent);

        return new EmployeeProfileDto
        {
            EmployeeId = employee.Id,
            FullName = employee.FullName,
            Department = employee.Department,
            Designation = employee.Designation,
            Allocations = allocations,
            TotalUtilisation = activeTotal
        };
    }

    public async Task<EmployeeReminderDto> GetReminderAsync(int userId)
    {
        var employee = await RequireEmployeeAsync(userId);
        var lastCompletedWeek = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);

        if (!await HadActiveAllocationDuringWeekAsync(employee.Id, lastCompletedWeek))
            return new EmployeeReminderDto { ShowReminder = false };

        if (await timesheetRepo.HasSubmittedForWeekAsync(employee.Id, lastCompletedWeek))
            return new EmployeeReminderDto { ShowReminder = false };

        return new EmployeeReminderDto
        {
            ShowReminder = true,
            MissingWeekStart = lastCompletedWeek,
            Message = $"Timesheet for week {lastCompletedWeek:dd-MMM-yyyy} has not been submitted."
        };
    }

    public async Task<EmployeeSubmitContextDto> GetSubmitContextAsync(int userId, DateTime? weekStart)
    {
        var employee = await RequireEmployeeAsync(userId);
        var config = await RequireConfigAsync();

        var weekMonday = weekStart.HasValue
            ? WeekDateHelper.GetMondayOfWeek(weekStart.Value)
            : WeekDateHelper.GetMondayOfWeek(DateTime.Today);
        WeekDateHelper.EnsureWeekNotInFuture(weekMonday);

        var allocations = await GetWeekAllocationsAsync(employee.Id, weekMonday, config.MaxWeeklyHours);
        var alreadySubmitted = await timesheetRepo.HasSubmittedForWeekAsync(employee.Id, weekMonday);

        return new EmployeeSubmitContextDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            WeekStart = weekMonday,
            WeekEnd = weekMonday.AddDays(6),
            MaxWeeklyHours = config.MaxWeeklyHours,
            AlreadySubmitted = alreadySubmitted,
            Allocations = allocations,
            ActivityTags = ActivityTags.All
        };
    }

    public async Task<TimesheetDto> SubmitTimesheetAsync(int userId, SubmitEmployeeTimesheetDto dto)
    {
        var employee = await RequireEmployeeAsync(userId);
        var config = await RequireConfigAsync();

        var weekMonday = WeekDateHelper.GetMondayOfWeek(dto.WeekStartDate);
        WeekDateHelper.EnsureWeekNotInFuture(weekMonday);

        if (await timesheetRepo.HasSubmittedForWeekAsync(employee.Id, weekMonday))
            throw new BusinessRuleException(ErrorMessages.TimesheetAlreadySubmitted);

        var weekAllocations = await GetWeekAllocationsAsync(employee.Id, weekMonday, config.MaxWeeklyHours);
        if (weekAllocations.Count == 0)
            throw new BusinessRuleException(ErrorMessages.NoAllocationsForWeek);

        ValidateProjectEntries(dto.Projects, weekAllocations, config.MaxWeeklyHours);

        var createDto = new CreateTimesheetDto
        {
            EmployeeId = employee.Id,
            WeekStartDate = weekMonday,
            Entries = dto.Projects
                .Where(p => p.Hours > 0)
                .Select(p => new CreateTimesheetEntryDto
                {
                    ProjectId = p.ProjectId,
                    Date = weekMonday,
                    Hours = p.Hours,
                    ActivityTags = p.ActivityTags
                })
                .ToList()
        };

        if (createDto.Entries.Count == 0)
            throw new BusinessRuleException(ErrorMessages.TimesheetHoursRequired);

        var existing = await timesheetRepo.GetByEmployeeAndWeekAsync(employee.Id, weekMonday);
        if (existing?.Status == TimesheetStatus.Missed)
            return await timesheetRepo.ReplaceEntriesAndSubmitAsync(existing.TimesheetId, createDto);

        if (existing is not null)
            throw new BusinessRuleException(ErrorMessages.TimesheetAlreadySubmitted);

        var timesheet = await timesheetRepo.CreateAsync(createDto);
        await timesheetRepo.SubmitAsync(timesheet.TimesheetId);
        return (await timesheetRepo.GetByIdAsync(timesheet.TimesheetId))!;
    }

    public async Task<IEnumerable<TimesheetDto>> GetMyTimesheetsAsync(int userId)
    {
        var employee = await RequireEmployeeAsync(userId);
        return (await timesheetRepo.GetByEmployeeIdAsync(employee.Id))
            .OrderByDescending(t => t.WeekStartDate);
    }

    public async Task<TimesheetDto?> GetMyTimesheetAsync(int userId, int timesheetId)
    {
        var employee = await RequireEmployeeAsync(userId);
        var timesheet = await timesheetRepo.GetByIdAsync(timesheetId);
        return timesheet?.EmployeeId == employee.Id ? timesheet : null;
    }

    public async Task<EmployeeSettingsDto> GetSettingsAsync()
    {
        var config = await RequireConfigAsync();
        return new EmployeeSettingsDto
        {
            MaxWeeklyHours = config.MaxWeeklyHours,
            ActivityTags = ActivityTags.All
        };
    }

    private static void ValidateProjectEntries(
        IReadOnlyList<SubmitTimesheetProjectEntryDto> entries,
        IReadOnlyList<EmployeeWeekAllocationDto> weekAllocations,
        int maxWeeklyHours)
    {
        if (entries.Count == 0)
            throw new BusinessRuleException(ErrorMessages.TimesheetEntryRequired);

        if (entries.Select(p => p.ProjectId).Distinct().Count() != entries.Count)
            throw new BusinessRuleException(ErrorMessages.DuplicateTimesheetProject);

        var allowedProjectIds = weekAllocations.Select(a => a.ProjectId).ToHashSet();
        var maxByProject = weekAllocations.ToDictionary(a => a.ProjectId, a => a.MaxHours);

        decimal totalHours = 0;
        foreach (var entry in entries)
        {
            if (entry.Hours < 0)
                throw new BusinessRuleException(ErrorMessages.NegativeHours);

            if (!allowedProjectIds.Contains(entry.ProjectId))
            {
                throw new BusinessRuleException(ErrorMessages.NotAllocatedToProject(entry.ProjectId));
            }

            if (entry.Hours > maxByProject[entry.ProjectId])
            {
                throw new BusinessRuleException(
                    ErrorMessages.ProjectHoursExceedMax(entry.ProjectId, maxByProject[entry.ProjectId]));
            }

            totalHours += entry.Hours;
        }

        ActivityTagValidator.ValidateEntryTags(
            entries.Select(e => (e.Hours, e.ActivityTags)));

        if (totalHours > maxWeeklyHours)
        {
            throw new BusinessRuleException(
                ErrorMessages.TotalHoursExceedWeeklyMax(totalHours, maxWeeklyHours));
        }
    }

    private async Task<Core.DTOs.Employee.EmployeeDto> RequireEmployeeAsync(int userId)
    {
        var employee = await employeeRepo.GetByUserIdAsync(userId);
        if (employee is null || !employee.IsActive)
            throw new BusinessRuleException(ErrorMessages.NoEmployeeProfile);
        return employee;
    }

    private async Task<Core.DTOs.Config.SystemConfigDto> RequireConfigAsync()
    {
        return await configRepo.GetAsync()
               ?? throw new BusinessRuleException(ErrorMessages.SystemConfigNotFound);
    }

    private async Task<List<EmployeeWeekAllocationDto>> GetWeekAllocationsAsync(
        int employeeId, DateTime weekMonday, int maxWeeklyHours)
    {
        var weekStart = DateOnly.FromDateTime(weekMonday);
        var weekEnd = DateOnly.FromDateTime(weekMonday.AddDays(6));

        return (await allocationRepo.GetByEmployeeIdAsync(employeeId))
            .Where(a => a.IsActive && a.FromDate <= weekEnd && a.ToDate >= weekStart)
            .Select(a => new EmployeeWeekAllocationDto
            {
                ProjectId = a.ProjectId,
                ProjectName = a.ProjectName,
                UtilisationPercent = a.UtilisationPercent,
                MaxHours = Math.Round((decimal)a.UtilisationPercent * maxWeeklyHours / AllocationLimits.MaxUtilisationPercent, 2),
                FromDate = a.FromDate,
                ToDate = a.ToDate
            })
            .ToList();
    }

    private async Task<bool> HadActiveAllocationDuringWeekAsync(int employeeId, DateTime weekMonday)
    {
        var weekStart = DateOnly.FromDateTime(weekMonday);
        var weekEnd = DateOnly.FromDateTime(weekMonday.AddDays(6));

        return (await allocationRepo.GetByEmployeeIdAsync(employeeId))
            .Any(a => a.IsActive && a.FromDate <= weekEnd && a.ToDate >= weekStart);
    }
}

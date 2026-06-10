using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

public class TimesheetService(
    ITimesheetRepository timesheetRepo,
    IProjectRepository projectRepo,
    IAllocationRepository allocationRepo,
    IEmployeeRepository employeeRepo) : ITimesheetService
{
    public async Task<IEnumerable<TimesheetDto>> GetTimesheetsByEmployeeAsync(int employeeId) =>
        await timesheetRepo.GetByEmployeeIdAsync(employeeId);

    public async Task<IEnumerable<TimesheetDto>> GetTimesheetsByWeekAsync(DateTime weekStart) =>
        await timesheetRepo.GetByWeekStartAsync(weekStart);

    public async Task<ManagerTeamTimesheetDto> GetTeamTimesheetsAsync(int managerUserId, DateTime weekStart)
    {
        var weekMonday = WeekDateHelper.GetMondayOfWeek(weekStart);
        var teamEmployeeIds = (await employeeRepo.GetTeamEmployeeIdsAsync(managerUserId)).ToHashSet();

        var projects = (await projectRepo.GetAllAsync())
            .Where(p => p.ManagerId == managerUserId)
            .ToList();

        var projectIds = projects.Select(p => p.Id).ToHashSet();
        var allTimesheets = (await timesheetRepo.GetByWeekStartAsync(weekMonday)).ToList();

        var submitted = allTimesheets
            .Where(t => teamEmployeeIds.Contains(t.EmployeeId) &&
                        t.Entries.Any(e => projectIds.Contains(e.ProjectId)))
            .ToList();

        var submittedEmployeeIds = submitted.Select(t => t.EmployeeId).ToHashSet();
        var missing = new List<MissingTimesheetEmployeeDto>();

        foreach (var project in projects)
        {
            var allocations = (await allocationRepo.GetByProjectIdAsync(project.Id))
                .Where(a => a.IsActive &&
                            teamEmployeeIds.Contains(a.EmployeeId) &&
                            a.FromDate <= DateOnly.FromDateTime(weekMonday.AddDays(6)) &&
                            a.ToDate >= DateOnly.FromDateTime(weekMonday))
                .ToList();

            foreach (var allocation in allocations)
            {
                if (submittedEmployeeIds.Contains(allocation.EmployeeId))
                    continue;

                if (missing.Any(m => m.EmployeeId == allocation.EmployeeId))
                    continue;

                missing.Add(new MissingTimesheetEmployeeDto
                {
                    EmployeeId = allocation.EmployeeId,
                    EmployeeName = allocation.EmployeeName,
                    ProjectName = project.Name
                });
            }
        }

        return new ManagerTeamTimesheetDto
        {
            WeekStart = weekMonday,
            Submitted = submitted,
            Missing = missing.OrderBy(m => m.EmployeeName).ToList()
        };
    }

    public async Task<TimesheetDto?> GetTimesheetByIdAsync(int timesheetId) =>
        await timesheetRepo.GetByIdAsync(timesheetId);

    public async Task<TimesheetDto?> GetTimesheetForManagerAsync(int managerUserId, int timesheetId)
    {
        var timesheet = await timesheetRepo.GetByIdAsync(timesheetId);
        if (timesheet is null)
            return null;

        if (!await employeeRepo.IsOnManagerTeamAsync(managerUserId, timesheet.EmployeeId))
            return null;

        var managerProjectIds = (await projectRepo.GetAllAsync())
            .Where(p => p.ManagerId == managerUserId)
            .Select(p => p.Id)
            .ToHashSet();

        var hasAccess = timesheet.Entries.Any(e => managerProjectIds.Contains(e.ProjectId));
        return hasAccess ? timesheet : null;
    }

    public async Task<TimesheetDto> CreateTimesheetAsync(CreateTimesheetDto dto) =>
        await timesheetRepo.CreateAsync(dto);

    public async Task<bool> SubmitTimesheetAsync(int timesheetId) =>
        await timesheetRepo.SubmitAsync(timesheetId);
}

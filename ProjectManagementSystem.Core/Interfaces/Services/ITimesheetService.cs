using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Timesheet;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface ITimesheetService
{
    Task<IEnumerable<TimesheetDto>> GetTimesheetsByEmployeeAsync(int employeeId);
    Task<IEnumerable<TimesheetDto>> GetTimesheetsByWeekAsync(DateTime weekStart);
    Task<ManagerTeamTimesheetDto> GetTeamTimesheetsAsync(int managerId, DateTime weekStart);
    Task<TimesheetDto?> GetTimesheetByIdAsync(int timesheetId);
    Task<TimesheetDto?> GetTimesheetForManagerAsync(int managerId, int timesheetId);
    Task<TimesheetDto> CreateTimesheetAsync(CreateTimesheetDto dto);
    Task<bool> SubmitTimesheetAsync(int timesheetId);
}

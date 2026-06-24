using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Timesheet;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IEmployeePortalService
{
    Task<EmployeeProfileDto> GetProfileAsync(int userId);
    Task<EmployeeReminderDto> GetReminderAsync(int userId);
    Task<EmployeeSubmitContextDto> GetSubmitContextAsync(int userId, DateTime? weekStart);
    Task<TimesheetDto> SubmitTimesheetAsync(int userId, SubmitEmployeeTimesheetDto dto);
    Task<IEnumerable<TimesheetDto>> GetMyTimesheetsAsync(int userId);
    Task<TimesheetDto?> GetMyTimesheetAsync(int userId, int timesheetId);
    Task<EmployeeSettingsDto> GetSettingsAsync();
}

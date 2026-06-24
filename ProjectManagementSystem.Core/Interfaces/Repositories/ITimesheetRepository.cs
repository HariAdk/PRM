using ProjectManagementSystem.Core.DTOs.Timesheet;

namespace ProjectManagementSystem.Core.Interfaces.Repositories;

public interface ITimesheetRepository
{
    Task<IEnumerable<TimesheetDto>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<TimesheetDto>> GetByWeekStartAsync(DateTime weekStart);
    Task<IEnumerable<string>> GetRecentActivityTagsAsync(int employeeId, int count = 5);
    Task<TimesheetDto?> GetByIdAsync(int timesheetId);
    Task<TimesheetDto?> GetByEmployeeAndWeekAsync(int employeeId, DateTime weekStart);
    Task<bool> HasSubmittedForWeekAsync(int employeeId, DateTime weekStart);
    Task<TimesheetDto> CreateAsync(CreateTimesheetDto dto);
    Task<bool> SubmitAsync(int timesheetId);
    Task<bool> ExistsForEmployeeWeekAsync(int employeeId, DateTime weekStart);
    Task CreateMissedAsync(int employeeId, DateTime weekStart);
    Task<TimesheetDto> ReplaceEntriesAndSubmitAsync(int timesheetId, CreateTimesheetDto dto);
}

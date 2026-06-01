using ProjectManagementSystem.Core.DTOs.Timesheet;

namespace ProjectManagementSystem.Core.DTOs.Manager;

public record ManagerTeamTimesheetDto
{
    public DateTime WeekStart { get; init; }
    public List<TimesheetDto> Submitted { get; init; } = new();
    public List<MissingTimesheetEmployeeDto> Missing { get; init; } = new();
}

public record MissingTimesheetEmployeeDto
{
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
}

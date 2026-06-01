using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Core.DTOs.Timesheet;

public record TimesheetDto
{
    public int TimesheetId { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime WeekStartDate { get; init; }
    public DateTime WeekEndDate { get; init; }
    public TimesheetStatus Status { get; init; }
    public decimal TotalHours { get; init; }
    public List<TimesheetEntryDto> Entries { get; init; } = new();
}

public record TimesheetEntryDto
{
    public int EntryId { get; init; }
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public decimal Hours { get; init; }
    public string ActivityTags { get; init; } = string.Empty;
}

public record CreateTimesheetDto
{
    public int EmployeeId { get; init; }
    public DateTime WeekStartDate { get; init; }
    public List<CreateTimesheetEntryDto> Entries { get; init; } = new();
}

public record CreateTimesheetEntryDto
{
    public int ProjectId { get; init; }
    public DateTime Date { get; init; }
    public decimal Hours { get; init; }
    public string ActivityTags { get; init; } = string.Empty;
}

public record SubmitTimesheetDto
{
    public int TimesheetId { get; init; }
}

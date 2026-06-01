namespace ProjectManagementSystem.Core.DTOs.Timesheet;

public record SubmitEmployeeTimesheetDto
{
    public DateTime WeekStartDate { get; init; }
    public List<SubmitTimesheetProjectEntryDto> Projects { get; init; } = new();
}

public record SubmitTimesheetProjectEntryDto
{
    public int ProjectId { get; init; }
    public decimal Hours { get; init; }
    public string ActivityTags { get; init; } = string.Empty;
}

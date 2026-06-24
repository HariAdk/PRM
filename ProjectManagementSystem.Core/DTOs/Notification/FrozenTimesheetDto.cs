namespace ProjectManagementSystem.Core.DTOs.Notification;

public record FrozenTimesheetDto
{
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime WeekStartDate { get; init; }
    public DateTime? FrozenAt { get; init; }
}

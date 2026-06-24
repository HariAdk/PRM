namespace ProjectManagementSystem.Core.DTOs.Notification;

public record TimesheetReminderStateDto
{
    public int EmployeeId { get; init; }
    public DateOnly WeekStartDate { get; init; }
    public int ReminderCount { get; init; }
    public DateOnly? LastReminderDate { get; init; }
    public bool IsFrozen { get; init; }
    public DateTime? FreezeNotifiedAt { get; init; }
    public DateTime? RestoredAt { get; init; }
}

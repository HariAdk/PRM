namespace ProjectManagementSystem.Infrastructure.Models;

public class TimesheetReminderState
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public int ReminderCount { get; set; }
    public DateOnly? LastReminderDate { get; set; }
    public bool IsFrozen { get; set; }
    public DateTime? FreezeNotifiedAt { get; set; }
    public DateTime? RestoredAt { get; set; }
    public int? RestoredByManagerId { get; set; }

    public Resource Resource { get; set; } = null!;
}

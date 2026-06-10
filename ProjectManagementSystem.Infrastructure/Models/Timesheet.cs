using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class Timesheet
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public decimal TotalHours { get; set; }
    public TimesheetStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Employee Employee { get; set; } = null!;
    public ICollection<TimesheetEntry> Entries { get; set; } = [];
}

using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class Resource
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ReportingManagerId { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Bench;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public User? ReportingManager { get; set; }
    public ICollection<ResourceSkill> Skills { get; set; } = [];
    public ICollection<Allocation> Allocations { get; set; } = [];
    public ICollection<Timesheet> Timesheets { get; set; } = [];
}

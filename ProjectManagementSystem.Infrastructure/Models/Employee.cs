using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class Employee
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Bench;
    public bool IsActive { get; set; } = true;
    public int? ManagerId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public User? ReportingManager { get; set; }
    public ICollection<EmployeeSkill> Skills { get; set; } = [];
    public ICollection<Allocation> Allocations { get; set; } = [];
    public ICollection<Timesheet> Timesheets { get; set; } = [];
}

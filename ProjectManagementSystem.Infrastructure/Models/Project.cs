using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class Project
{
    public int Id { get; set; }
    public int ManagerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    public ProjectHealth HealthStatus { get; set; } = ProjectHealth.OnTrack;

    // Navigation
    public User Manager { get; set; } = null!;
    public ICollection<Milestone> Milestones { get; set; } = [];
    public ICollection<Allocation> Allocations { get; set; } = [];
    public ICollection<TimesheetEntry> TimesheetEntries { get; set; } = [];
}

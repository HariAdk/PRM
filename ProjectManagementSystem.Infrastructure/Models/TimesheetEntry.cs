namespace ProjectManagementSystem.Infrastructure.Models;

public class TimesheetEntry
{
    public int Id { get; set; }
    public int TimesheetId { get; set; }
    public int ProjectId { get; set; }
    public decimal Hours { get; set; }

    /// <summary>Comma-separated activity tags e.g. "Backend API,Bug Fixing".</summary>
    public string ActivityTags { get; set; } = string.Empty;

    // Navigation
    public Timesheet Timesheet { get; set; } = null!;
    public Project Project { get; set; } = null!;
}

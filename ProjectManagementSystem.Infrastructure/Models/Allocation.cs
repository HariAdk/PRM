namespace ProjectManagementSystem.Infrastructure.Models;

public class Allocation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }

    /// <summary>Percentage of the employee's time on this project (1–100).</summary>
    public int UtilisationPercent { get; set; }

    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Employee Employee { get; set; } = null!;
    public Project Project { get; set; } = null!;
}

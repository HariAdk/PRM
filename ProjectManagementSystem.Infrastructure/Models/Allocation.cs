namespace ProjectManagementSystem.Infrastructure.Models;

public class Allocation
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int ProjectId { get; set; }
    public int UtilisationPercent { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Resource Resource { get; set; } = null!;
    public Project Project { get; set; } = null!;
}

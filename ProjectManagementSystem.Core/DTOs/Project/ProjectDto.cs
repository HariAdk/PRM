namespace ProjectManagementSystem.Core.DTOs.Project;

public class ProjectDto
{
    public int Id { get; set; }
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
}

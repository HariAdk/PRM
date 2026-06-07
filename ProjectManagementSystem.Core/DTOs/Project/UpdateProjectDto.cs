namespace ProjectManagementSystem.Core.DTOs.Project;

public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public int TotalStoryPoints { get; set; }
}

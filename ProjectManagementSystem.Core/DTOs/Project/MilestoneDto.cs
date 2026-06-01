namespace ProjectManagementSystem.Core.DTOs.Project;

public class MilestoneDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

namespace ProjectManagementSystem.Core.DTOs.Project;

public class CreateMilestoneDto
{
    public string Title { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public int StoryPoints { get; set; }
}

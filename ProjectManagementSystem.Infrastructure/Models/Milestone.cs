using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class Milestone
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.NotStarted;
    public int StoryPoints { get; set; }
    public Project Project { get; set; } = null!;
}

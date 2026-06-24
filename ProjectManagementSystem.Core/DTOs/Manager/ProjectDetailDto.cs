using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Project;

namespace ProjectManagementSystem.Core.DTOs.Manager;

public record ProjectDetailDto
{
    public ProjectDto Project { get; init; } = null!;
    public string DisplayHealth { get; init; } = string.Empty;
    public List<RiskFlagDto> RiskFlags { get; init; } = new();
    public List<ProjectMilestoneDetailDto> Milestones { get; init; } = new();
    public List<AllocationDto> Allocations { get; init; } = new();
}

public record RiskFlagDto
{
    public string Message { get; init; } = string.Empty;
    public bool IsPositive { get; init; }
}

public record ProjectMilestoneDetailDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateOnly DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsOverdue { get; init; }
    public int DaysOverdue { get; init; }
}

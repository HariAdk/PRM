namespace ProjectManagementSystem.Core.DTOs.AI;

/// <summary>Pre-filtered employee snapshot sent to the LLM adapter.</summary>
public record SkillMatchCandidateDto
{
    public int EmployeeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Skills { get; init; } = string.Empty;
    public decimal AvailabilityPercent { get; init; }
    public decimal FreeHoursPerWeek { get; init; }
    public string RecentActivity { get; init; } = string.Empty;
}

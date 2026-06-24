namespace ProjectManagementSystem.Core.DTOs.AI;

public record TeamBuildCandidateDto
{
    public int EmployeeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public decimal AvailabilityPercent { get; init; }
    public string SkillsWithProficiency { get; init; } = string.Empty;
    public string RecentActivity { get; init; } = string.Empty;
}

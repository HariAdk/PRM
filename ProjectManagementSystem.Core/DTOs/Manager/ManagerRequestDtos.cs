namespace ProjectManagementSystem.Core.DTOs.Manager;

public record EndAllocationDto
{
    public DateOnly EndDate { get; init; }
}

public record AISkillMatchRequestDto
{
    public int ProjectId { get; init; }
    public string Requirement { get; init; } = string.Empty;
}

public record AISkillMatchResultDto
{
    public List<AIMatchedEmployeeDto> Matches { get; init; } = new();
}

public record AIMatchedEmployeeDto
{
    public int EmployeeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SkillsMatch { get; init; } = string.Empty;
    public decimal AvailabilityPercentage { get; init; }
    public string RecentActivity { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record AIRiskSummaryRequestDto
{
    public int ProjectId { get; init; }
}

public record AIRiskSummaryResultDto
{
    public string Summary { get; init; } = string.Empty;
}

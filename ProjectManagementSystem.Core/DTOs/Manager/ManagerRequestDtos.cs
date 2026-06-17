namespace ProjectManagementSystem.Core.DTOs.Manager;

public record EndAllocationDto
{
    public DateOnly EndDate { get; init; }
}

public record AISkillMatchRequestDto
{
    public string Requirement { get; init; } = string.Empty;
}

public record AISkillMatchResultDto
{
    public List<AIMatchedEmployeeDto> Matches { get; init; } = new();
    public bool UsedFallback { get; init; }
    public string? FallbackReason { get; init; }
}

public record AIMatchedEmployeeDto
{
    public int EmployeeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public string SkillsMatch { get; init; } = string.Empty;
    public decimal AvailabilityPercentage { get; init; }
    public string RecentActivity { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record AITeamBuildRequestDto
{
    public string Requirement { get; init; } = string.Empty;
}

public record AITeamBuildResultDto
{
    public List<TeamBuildRoleSuggestionDto> Roles { get; init; } = [];
    public bool UsedFallback { get; init; }
    public string? FallbackReason { get; init; }
}

public record TeamBuildRoleSuggestionDto
{
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int? EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public string SkillsMatch { get; init; } = string.Empty;
    public decimal AvailabilityPercentage { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record RestoreTimesheetAccessDto
{
    public int EmployeeId { get; init; }
    public DateTime WeekStartDate { get; init; }
}

public record AIRiskSummaryRequestDto
{
    public int ProjectId { get; init; }
}

public record AIRiskSummaryResultDto
{
    public string Summary { get; init; } = string.Empty;
}

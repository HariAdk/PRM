namespace ProjectManagementSystem.Core.DTOs.Manager;

public record ResourceDashboardDto
{
    public List<BenchEmployeeDto> BenchEmployees { get; init; } = new();
    public List<ActiveEmployeeDto> ActiveEmployees { get; init; } = new();
    public int BenchCount { get; init; }
    public int OverUtilisedCount { get; init; }
    public int PartialCount { get; init; }
}

public record BenchEmployeeDto
{
    public int EmployeeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Skills { get; init; } = string.Empty;
}

public record ActiveEmployeeDto
{
    public int EmployeeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal AllocationPercentage { get; init; }
    public decimal AvailabilityPercentage { get; init; }
    public string AvailabilityStatus { get; init; } = string.Empty;
}

public record EmployeeDetailDto
{
    public int EmployeeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string CurrentStatus { get; init; } = string.Empty;
    public decimal CurrentAllocation { get; init; }
    public string Skills { get; init; } = string.Empty;
    public List<AllocationDetailDto> ActiveAllocations { get; init; } = new();
    public List<string> RecentActivityTags { get; init; } = new();
}

public record AllocationDetailDto
{
    public string ProjectName { get; init; } = string.Empty;
    public decimal Percentage { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
}

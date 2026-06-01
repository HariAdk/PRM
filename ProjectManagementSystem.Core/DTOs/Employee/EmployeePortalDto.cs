using ProjectManagementSystem.Core.DTOs.Allocation;

namespace ProjectManagementSystem.Core.DTOs.Employee;

public record EmployeeReminderDto
{
    public bool ShowReminder { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime? MissingWeekStart { get; init; }
}

public record EmployeeSettingsDto
{
    public int MaxWeeklyHours { get; init; }
    public string[] ActivityTags { get; init; } = [];
}

public record EmployeeWeekAllocationDto
{
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public int UtilisationPercent { get; init; }
    public decimal MaxHours { get; init; }
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
}

public record EmployeeSubmitContextDto
{
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime WeekStart { get; init; }
    public DateTime WeekEnd { get; init; }
    public int MaxWeeklyHours { get; init; }
    public bool AlreadySubmitted { get; init; }
    public List<EmployeeWeekAllocationDto> Allocations { get; init; } = new();
    public string[] ActivityTags { get; init; } = [];
}

public record EmployeeProfileDto
{
    public int EmployeeId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public List<AllocationDto> Allocations { get; init; } = new();
    public int TotalUtilisation { get; init; }
}

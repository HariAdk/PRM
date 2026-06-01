namespace ProjectManagementSystem.Core.DTOs.Config;

public record SystemConfigDto
{
    public string LlmProvider { get; init; } = string.Empty;
    public string LlmApiKey { get; init; } = string.Empty;
    public int SchedulerIntervalHours { get; init; }
    public int MaxWeeklyHours { get; init; }
}


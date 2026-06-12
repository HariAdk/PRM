namespace ProjectManagementSystem.Core.DTOs.Config;

public record SystemConfigDto
{
    public string LlmProvider { get; init; } = string.Empty;
    public string LlmApiKey { get; init; } = string.Empty;
    public int SchedulerIntervalHours { get; init; }
    public int MaxWeeklyHours { get; init; }
    public bool EmailEnabled { get; init; }
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public string SmtpUsername { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
    public string EmailFromAddress { get; init; } = string.Empty;
}


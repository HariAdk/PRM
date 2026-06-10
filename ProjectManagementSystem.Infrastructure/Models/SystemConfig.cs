using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Infrastructure.Models;

public class SystemConfig
{
    public int Id { get; set; }
    public string LlmProvider { get; set; } = LlmProviders.Gemini;
    public string LlmApiKey { get; set; } = string.Empty;

    /// <summary>How often the background scheduler runs (in hours).</summary>
    public int SchedulerIntervalHours { get; set; } = SystemDefaults.SchedulerIntervalHours;

    /// <summary>Maximum hours an employee can log per week.</summary>
    public int MaxWeeklyHours { get; set; } = SystemDefaults.MaxWeeklyHours;
}

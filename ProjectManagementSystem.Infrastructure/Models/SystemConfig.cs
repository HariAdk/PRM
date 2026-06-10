using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Infrastructure.Models;

public class SystemConfig
{
    public int Id { get; set; }
    public string LlmProvider { get; set; } = LlmProviders.Gemini;
    public string LlmApiKey { get; set; } = string.Empty;
    public int SchedulerIntervalHours { get; set; } = SystemDefaults.SchedulerIntervalHours;
    public int MaxWeeklyHours { get; set; } = SystemDefaults.MaxWeeklyHours;
}

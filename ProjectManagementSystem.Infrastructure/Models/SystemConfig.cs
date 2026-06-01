namespace ProjectManagementSystem.Infrastructure.Models;

public class SystemConfig
{
    public int Id { get; set; }
    public string LlmProvider { get; set; } = "Gemini";
    public string LlmApiKey { get; set; } = string.Empty;

    /// <summary>How often the background scheduler runs (in hours). Default: 4.</summary>
    public int SchedulerIntervalHours { get; set; } = 4;

    /// <summary>Maximum hours an employee can log per week. Default: 40.</summary>
    public int MaxWeeklyHours { get; set; } = 40;
}

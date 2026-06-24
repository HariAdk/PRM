namespace ProjectManagementSystem.Core.DTOs.Notification;

public record AtRiskProjectEmailDto
{
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string ManagerName { get; init; } = string.Empty;
    public string ManagerEmail { get; init; } = string.Empty;
    public string HealthStatus { get; init; } = string.Empty;
    public string MilestoneSummary { get; init; } = string.Empty;
}

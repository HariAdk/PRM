namespace ProjectManagementSystem.Core.Interfaces.Services;

/// <summary>
/// Email and escalation processing. Invoked only by the background scheduler — never from API or client requests.
/// </summary>
public interface INotificationService
{
    Task ProcessTimesheetEscalationsAsync(CancellationToken cancellationToken = default);
    Task ProcessAtRiskProjectNotificationsAsync(CancellationToken cancellationToken = default);
}

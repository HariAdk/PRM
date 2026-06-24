namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface ISchedulerService
{
    Task RunScheduledTasksAsync(CancellationToken cancellationToken = default);
}

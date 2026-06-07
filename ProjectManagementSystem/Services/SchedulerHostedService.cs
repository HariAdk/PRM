using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Services;

/// <summary>
/// Background worker that runs scheduler tasks on startup and every N hours
/// (interval read from system_config.scheduler_interval_hours).
/// </summary>
public class SchedulerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<SchedulerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background scheduler started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);

            var delayHours = await GetIntervalHoursAsync(stoppingToken);
            logger.LogInformation("Next scheduler run in {Hours} hour(s)", delayHours);

            try
            {
                await Task.Delay(TimeSpan.FromHours(delayHours), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("Background scheduler stopped");
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var scheduler = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
            await scheduler.RunScheduledTasksAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Scheduler run failed");
        }
    }

    private async Task<int> GetIntervalHoursAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var configRepo = scope.ServiceProvider.GetRequiredService<ISystemConfigRepository>();
        var config = await configRepo.GetAsync();
        var hours = config?.SchedulerIntervalHours ?? SystemDefaults.SchedulerIntervalHours;
        return hours < SystemDefaults.MinSchedulerIntervalHours ? SystemDefaults.SchedulerIntervalHours : hours;
    }
}

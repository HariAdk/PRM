using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class SystemConfigRepository(AppDbContext db) : ISystemConfigRepository
{
    public async Task<SystemConfigDto?> GetAsync()
    {
        var config = await db.SystemConfigs.FirstOrDefaultAsync();
        return config is null ? null : MapToDto(config);
    }

    public async Task UpdateAsync(SystemConfigDto dto)
    {
        var config = await db.SystemConfigs.FirstOrDefaultAsync()
                     ?? throw new InvalidOperationException("SystemConfig not found.");
        config.LlmProvider = dto.LlmProvider;
        config.LlmApiKey = dto.LlmApiKey;
        config.SchedulerIntervalHours = dto.SchedulerIntervalHours;
        config.MaxWeeklyHours = dto.MaxWeeklyHours;
        await db.SaveChangesAsync();
    }

    private static SystemConfigDto MapToDto(SystemConfig c) => new()
    {
        LlmProvider = c.LlmProvider,
        LlmApiKey = c.LlmApiKey,
        SchedulerIntervalHours = c.SchedulerIntervalHours,
        MaxWeeklyHours = c.MaxWeeklyHours
    };
}

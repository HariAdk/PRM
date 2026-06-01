using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Data;

/// <summary>
/// Runs once at application startup.
/// Seeds the first Admin account and the default SystemConfig row
/// only if they do not already exist — safe to leave in production.
/// </summary>
public class DatabaseSeeder(AppDbContext db, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync()
    {
        await SeedAdminAsync();
        await SeedSystemConfigAsync();
    }

    private async Task SeedAdminAsync()
    {
        bool adminExists = await db.Users.AnyAsync(u => u.Role == UserRole.Admin);
        if (adminExists)
        {
            logger.LogInformation("Admin account already exists — skipping seed.");
            return;
        }

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@prm.local",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
            Role = UserRole.Admin,
            IsActive = true,
            ForcePasswordChange = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Bootstrap Admin created. Username: admin | Temp password: Admin@1234 " +
            "— user must change password on first login.");
    }

    private async Task SeedSystemConfigAsync()
    {
        bool configExists = await db.SystemConfigs.AnyAsync();
        if (configExists)
        {
            logger.LogInformation("SystemConfig already exists — skipping seed.");
            return;
        }

        var config = new SystemConfig
        {
            LlmProvider = "Gemini",
            LlmApiKey = string.Empty,
            SchedulerIntervalHours = 4,
            MaxWeeklyHours = 40
        };

        db.SystemConfigs.Add(config);
        await db.SaveChangesAsync();

        logger.LogInformation("Default SystemConfig seeded.");
    }
}

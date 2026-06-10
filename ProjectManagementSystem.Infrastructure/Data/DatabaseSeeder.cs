using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Data;

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
            FullName = BootstrapDefaults.AdminFullName,
            Email = BootstrapDefaults.AdminEmail,
            Username = BootstrapDefaults.AdminUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(BootstrapDefaults.AdminTemporaryPassword),
            Role = UserRole.Admin,
            IsActive = true,
            ForcePasswordChange = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Bootstrap Admin created. Username: {Username} | Temp password: {Password} — user must change password on first login.",
            BootstrapDefaults.AdminUsername,
            BootstrapDefaults.AdminTemporaryPassword);
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
            LlmProvider = LlmProviders.Gemini,
            LlmApiKey = string.Empty,
            SchedulerIntervalHours = SystemDefaults.SchedulerIntervalHours,
            MaxWeeklyHours = SystemDefaults.MaxWeeklyHours
        };

        db.SystemConfigs.Add(config);
        await db.SaveChangesAsync();

        logger.LogInformation("Default SystemConfig seeded.");
    }
}

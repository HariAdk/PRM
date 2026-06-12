using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Settings;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Data;

public class DatabaseSeeder(AppDbContext db, IConfiguration configuration, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
        await SeedSystemConfigAsync();
        await EmailConfigSeeder.ApplyAsync(db, configuration, logger);
    }

    private async Task SeedRolesAsync()
    {
        if (await db.Roles.AnyAsync()) return;

        var now = DateTime.UtcNow;
        db.Roles.AddRange(
            new Role { Name = RoleNames.Admin, Description = "System administrator", CreatedAt = now, UpdatedAt = now },
            new Role { Name = RoleNames.Manager, Description = "Project / team manager", CreatedAt = now, UpdatedAt = now },
            new Role { Name = RoleNames.Employee, Description = "Individual contributor", CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();
        logger.LogInformation("Roles seeded.");
    }

    private async Task SeedAdminAsync()
    {
        var adminRoleId = await RoleResolver.GetRoleIdAsync(db, RoleNames.Admin);
        if (await db.Users.AnyAsync(u => u.RoleId == adminRoleId))
        {
            logger.LogInformation("Admin account already exists ù skipping seed.");
            return;
        }

        var now = DateTime.UtcNow;
        db.Users.Add(new User
        {
            FullName = BootstrapDefaults.AdminFullName,
            Email = BootstrapDefaults.AdminEmail,
            Username = BootstrapDefaults.AdminUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(BootstrapDefaults.AdminTemporaryPassword),
            RoleId = adminRoleId,
            IsActive = true,
            IsForcePasswordChange = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Bootstrap Admin created. Username: {Username} | Temp password: {Password}",
            BootstrapDefaults.AdminUsername,
            BootstrapDefaults.AdminTemporaryPassword);
    }

    private async Task SeedSystemConfigAsync()
    {
        if (await db.SystemConfigs.AnyAsync())
        {
            logger.LogInformation("SystemConfig already exists ù skipping seed.");
            return;
        }

        var smtp = configuration.GetSection("Email:Smtp").Get<SmtpSettings>();
        db.SystemConfigs.Add(new SystemConfig
        {
            LlmProvider = LlmProviders.Ollama,
            LlmApiKey = string.Empty,
            SchedulerIntervalHours = SystemDefaults.SchedulerIntervalHours,
            MaxWeeklyHours = SystemDefaults.MaxWeeklyHours,
            EmailEnabled = smtp?.Enabled ?? false,
            SmtpHost = smtp?.Host ?? string.Empty,
            SmtpPort = smtp?.Port > 0 ? smtp.Port : SystemDefaults.SmtpPort,
            SmtpUsername = smtp?.Username ?? string.Empty,
            SmtpPassword = smtp?.Password ?? string.Empty,
            EmailFromAddress = string.IsNullOrWhiteSpace(smtp?.FromAddress)
                ? smtp?.Username ?? string.Empty
                : smtp.FromAddress
        });
        await db.SaveChangesAsync();
        logger.LogInformation("Default SystemConfig seeded.");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Settings;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Data;

public static class EmailConfigSeeder
{
    public static async Task ApplyAsync(
        AppDbContext db,
        IConfiguration configuration,
        ILogger logger)
    {
        var emailSection = configuration.GetSection("Email");
        if (!emailSection.GetValue<bool>("SeedSmtpToDatabase"))
            return;

        var smtp = emailSection.GetSection("Smtp").Get<SmtpSettings>();
        if (smtp is null || !smtp.Enabled || string.IsNullOrWhiteSpace(smtp.Host))
            return;

        var config = await db.SystemConfigs.FirstOrDefaultAsync();
        if (config is null)
            return;

        if (!string.IsNullOrWhiteSpace(config.SmtpHost))
        {
            logger.LogDebug("SystemConfig SMTP already set; skipping appsettings email seed.");
            return;
        }

        config.EmailEnabled = true;
        config.SmtpHost = smtp.Host;
        config.SmtpPort = smtp.Port > 0 ? smtp.Port : EmailDefaults.GmailSmtpPort;
        config.SmtpUsername = smtp.Username;
        config.SmtpPassword = smtp.Password;
        config.EmailFromAddress = string.IsNullOrWhiteSpace(smtp.FromAddress)
            ? smtp.Username
            : smtp.FromAddress;

        await db.SaveChangesAsync();
        logger.LogInformation(
            "SMTP settings copied from appsettings to SystemConfig ({Host}, user {User}).",
            config.SmtpHost, config.SmtpUsername);
    }
}

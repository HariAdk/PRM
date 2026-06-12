using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Core.Settings;

namespace ProjectManagementSystem.Infrastructure.Email;

public class SmtpEmailService(
    ISystemConfigRepository configRepo,
    IOptions<SmtpSettings> smtpDefaults,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(string toEmail, string subject, string body)
    {
        var resolved = await ResolveSmtpAsync();
        if (resolved is null)
        {
            logger.LogInformation(
                "Email not configured. Would send to {To}: {Subject}",
                toEmail, subject);
            return;
        }

        using var client = new SmtpClient(resolved.Host, resolved.Port)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrWhiteSpace(resolved.Username)
                ? null
                : new NetworkCredential(resolved.Username, resolved.Password)
        };

        var from = string.IsNullOrWhiteSpace(resolved.FromDisplayName)
            ? new MailAddress(resolved.FromAddress)
            : new MailAddress(resolved.FromAddress, resolved.FromDisplayName);

        using var message = new MailMessage
        {
            From = from,
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(new MailAddress(toEmail));

        await client.SendMailAsync(message);
        logger.LogInformation("Email sent to {To}: {Subject}", toEmail, subject);
    }

    private async Task<ResolvedSmtp?> ResolveSmtpAsync()
    {
        var db = await configRepo.GetAsync();
        if (db?.EmailEnabled == true && !string.IsNullOrWhiteSpace(db.SmtpHost))
            return FromDatabase(db);

        var defaults = smtpDefaults.Value;
        if (defaults.Enabled && !string.IsNullOrWhiteSpace(defaults.Host))
            return FromAppSettings(defaults);

        return null;
    }

    private static ResolvedSmtp FromDatabase(SystemConfigDto db) => new()
    {
        Host = db.SmtpHost,
        Port = db.SmtpPort,
        Username = db.SmtpUsername,
        Password = db.SmtpPassword,
        FromAddress = db.EmailFromAddress,
        FromDisplayName = string.Empty
    };

    private static ResolvedSmtp FromAppSettings(SmtpSettings settings) => new()
    {
        Host = settings.Host,
        Port = settings.Port,
        Username = settings.Username,
        Password = settings.Password,
        FromAddress = settings.FromAddress,
        FromDisplayName = settings.FromDisplayName
    };

    private sealed class ResolvedSmtp
    {
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string FromAddress { get; init; } = string.Empty;
        public string FromDisplayName { get; init; } = string.Empty;
    }
}

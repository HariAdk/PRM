using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Core.Settings;

namespace ProjectManagementSystem.Infrastructure.Email;

/// <summary>
/// Writes emails as .txt files for local testing without SMTP.
/// </summary>
public class MockEmailService(
    IOptions<EmailSettings> options,
    ILogger<MockEmailService> logger) : IEmailService
{
    public async Task SendAsync(string toEmail, string subject, string body)
    {
        var outboxDir = Path.IsPathRooted(options.Value.OutboxPath)
            ? options.Value.OutboxPath
            : Path.Combine(Directory.GetCurrentDirectory(), options.Value.OutboxPath);
        Directory.CreateDirectory(outboxDir);

        var safeTo = string.Concat(toEmail.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{safeTo}.txt";
        var filePath = Path.Combine(outboxDir, fileName);

        var content =
            $"""
            To: {toEmail}
            Subject: {subject}
            Sent (UTC): {DateTime.UtcNow:u}

            {body}
            """;

        await File.WriteAllTextAsync(filePath, content);
        logger.LogInformation("Mock email saved: {FilePath}", filePath);
    }
}

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string body);
}

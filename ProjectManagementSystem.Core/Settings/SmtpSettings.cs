using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Core.Settings;

public class SmtpSettings
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = SystemDefaults.SmtpPort;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = string.Empty;
}

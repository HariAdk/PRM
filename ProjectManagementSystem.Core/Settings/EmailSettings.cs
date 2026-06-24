namespace ProjectManagementSystem.Core.Settings;

public class EmailSettings
{
    /// <summary>Smtp = real SMTP. Mock = save emails to local outbox folder (no server needed).</summary>
    public string Provider { get; set; } = "Smtp";

    /// <summary>Folder under the API content root where mock emails are written.</summary>
    public string OutboxPath { get; set; } = "email-outbox";

    /// <summary>When true, copy appsettings Smtp values into SystemConfig if DB SMTP host is empty.</summary>
    public bool SeedSmtpToDatabase { get; set; } = true;

    public SmtpSettings Smtp { get; set; } = new();
}

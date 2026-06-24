using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Config;

namespace ProjectManagementSystem.Client.Screens.Admin;

/// <summary>Screen 3.5 � System Configuration</summary>
public class SystemConfigScreen(ApiClient api) : IScreen
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("SYSTEM CONFIGURATION");

            var (config, err) = await api.GetConfigAsync();
            if (err is not null) { ConsoleUI.Error(err); ConsoleUI.PressAnyKey(); return; }

            Console.WriteLine("Current Settings:");
            Console.WriteLine($"  LLM Provider        :  {config!.LlmProvider}");
            Console.WriteLine($"  LLM API Key         :  {config.LlmApiKey}");
            if (config.LlmProvider.Equals(LlmProviders.Ollama, StringComparison.OrdinalIgnoreCase))
                Console.WriteLine("  (Ollama: store the apikey header value in LLM API Key)");
            Console.WriteLine($"  Scheduler Interval  :  {config.SchedulerIntervalHours} hours");
            Console.WriteLine($"  Max Weekly Hours    :  {config.MaxWeeklyHours}");
            Console.WriteLine($"  Email Enabled       :  {config.EmailEnabled}");
            Console.WriteLine($"  SMTP Host           :  {config.SmtpHost}");
            Console.WriteLine($"  SMTP Port           :  {config.SmtpPort}");
            Console.WriteLine($"  SMTP Username       :  {config.SmtpUsername}");
            Console.WriteLine($"  SMTP Password       :  {config.SmtpPassword}");
            Console.WriteLine($"  From Address        :  {config.EmailFromAddress}");
            ConsoleUI.Divider();
            ConsoleUI.Menu(1, "Update LLM API Key");
            ConsoleUI.Menu(2, "Change LLM Provider  (Gemini / Groq / Ollama)");
            ConsoleUI.Menu(3, "Update Scheduler Interval");
            ConsoleUI.Menu(4, "Update Max Weekly Hours");
            ConsoleUI.Menu(5, "Configure Email (SMTP)");
            ConsoleUI.Menu(6, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1":
                    var key = ConsoleUI.PromptPassword("New LLM API Key");
                    await SaveConfigAsync(config with { LlmApiKey = key });
                    break;
                case "2":
                    Console.WriteLine("\nProvider: (1) Gemini   (2) Groq   (3) Ollama (Gemma)");
                    var pc = ConsoleUI.Prompt("Enter choice");
                    var provider = pc switch
                    {
                        "2" => LlmProviders.Groq,
                        "3" => LlmProviders.Ollama,
                        _ => LlmProviders.Gemini
                    };
                    await SaveConfigAsync(config with { LlmProvider = provider });
                    break;
                case "3":
                    var interval = ConsoleUI.Prompt("Scheduler interval (hours)");
                    if (int.TryParse(interval, out var iv))
                        await SaveConfigAsync(config with { SchedulerIntervalHours = iv });
                    else ConsoleUI.Error("Invalid number.");
                    ConsoleUI.PressAnyKey();
                    break;
                case "4":
                    var maxHrs = ConsoleUI.Prompt("Max weekly hours");
                    if (int.TryParse(maxHrs, out var mh))
                        await SaveConfigAsync(config with { MaxWeeklyHours = mh });
                    else ConsoleUI.Error("Invalid number.");
                    ConsoleUI.PressAnyKey();
                    break;
                case "5":
                    await ConfigureEmailAsync(config!);
                    break;
                case "6": return;
                default:  ConsoleUI.Error("Invalid option."); ConsoleUI.PressAnyKey(); break;
            }
        }
    }

    private async Task ConfigureEmailAsync(SystemConfigDto config)
    {
        Console.WriteLine($"Current email enabled: {config.EmailEnabled}");
        var enabled = ConsoleUI.Prompt("Enable email notifications? (y/n)");
        var host = ConsoleUI.Prompt($"SMTP host [{config.SmtpHost}]");
        var portStr = ConsoleUI.Prompt($"SMTP port [{config.SmtpPort}]");
        var user = ConsoleUI.Prompt($"SMTP username [{config.SmtpUsername}]");
        var pass = ConsoleUI.PromptPassword("SMTP password (Enter to keep current)");
        var from = ConsoleUI.Prompt($"From email address [{config.EmailFromAddress}]");

        if (!int.TryParse(portStr, out var port))
        {
            ConsoleUI.Error("Invalid port.");
            ConsoleUI.PressAnyKey();
            return;
        }

        await SaveConfigAsync(config with
        {
            EmailEnabled = string.IsNullOrWhiteSpace(enabled)
                ? config.EmailEnabled
                : enabled.Equals("y", StringComparison.OrdinalIgnoreCase),
            SmtpHost = string.IsNullOrWhiteSpace(host) ? config.SmtpHost : host,
            SmtpPort = port,
            SmtpUsername = string.IsNullOrWhiteSpace(user) ? config.SmtpUsername : user,
            SmtpPassword = string.IsNullOrEmpty(pass) ? config.SmtpPassword : pass,
            EmailFromAddress = string.IsNullOrWhiteSpace(from) ? config.EmailFromAddress : from
        });
    }

    private async Task SaveConfigAsync(SystemConfigDto dto)
    {
        var (_, error) = await api.UpdateConfigAsync(dto);
        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success(SuccessMessages.ConfigurationUpdated);
        ConsoleUI.PressAnyKey();
    }
}

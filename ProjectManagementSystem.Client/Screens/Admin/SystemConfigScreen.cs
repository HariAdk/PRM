using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.DTOs.Config;

namespace ProjectManagementSystem.Client.Screens.Admin;

/// <summary>Screen 3.5 — System Configuration</summary>
public class SystemConfigScreen(ApiClient api)
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
            Console.WriteLine($"  Scheduler Interval  :  {config.SchedulerIntervalHours} hours");
            Console.WriteLine($"  Max Weekly Hours    :  {config.MaxWeeklyHours}");
            ConsoleUI.Divider();
            ConsoleUI.Menu(1, "Update LLM API Key");
            ConsoleUI.Menu(2, "Change LLM Provider  (Gemini / Groq)");
            ConsoleUI.Menu(3, "Update Scheduler Interval");
            ConsoleUI.Menu(4, "Update Max Weekly Hours");
            ConsoleUI.Menu(5, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1":
                    var key = ConsoleUI.PromptPassword("New LLM API Key");
                    await SaveConfigAsync(config with { LlmApiKey = key });
                    break;
                case "2":
                    Console.WriteLine("\nProvider: (1) Gemini   (2) Groq");
                    var pc = ConsoleUI.Prompt("Enter choice");
                    var provider = pc == "2" ? "Groq" : "Gemini";
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
                case "5": return;
                default:  ConsoleUI.Error("Invalid option."); ConsoleUI.PressAnyKey(); break;
            }
        }
    }

    private async Task SaveConfigAsync(SystemConfigDto dto)
    {
        var (_, error) = await api.UpdateConfigAsync(dto);
        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("Configuration updated.");
        ConsoleUI.PressAnyKey();
    }
}

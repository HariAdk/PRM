namespace ProjectManagementSystem.Client.Helpers;

using ProjectManagementSystem.Core.DTOs.Manager;

/// <summary>
/// Central rendering helper matching the BRD console layouts in Project_Management.md.
/// </summary>
public static class ConsoleUI
{
    public const int BoxWidth = 46;

    private const char TL = '\u2554';
    private const char TR = '\u2557';
    private const char BL = '\u255A';
    private const char BR = '\u255D';
    private const char H  = '\u2550';
    private const char V  = '\u2551';
    private const char SH = '\u2500';

    public static void DrawBox(string title, string? subtitle = null)
    {
        Console.WriteLine();
        Console.WriteLine($"{TL}{new string(H, BoxWidth)}{TR}");
        WriteBoxLine(title);
        if (subtitle is not null)
            WriteBoxLine(subtitle);
        Console.WriteLine($"{BL}{new string(H, BoxWidth)}{BR}");
        Console.WriteLine();
    }

    private static void WriteBoxLine(string text)
    {
        var line = ("    " + text).PadRight(BoxWidth)[..BoxWidth];
        Console.WriteLine($"{V}{line}{V}");
    }

    public static void Divider() =>
        Console.WriteLine(new string(SH, BoxWidth));

    public static void BlankLine() => Console.WriteLine();

    public static void Menu(int number, string label) =>
        Console.WriteLine($"{number}. {label}");

    public static void SubHeader(string title)
    {
        var prefix = $"\u2500\u2500 {title} ";
        var dashes = Math.Max(1, BoxWidth - prefix.Length);
        Console.WriteLine(prefix + new string(SH, dashes));
    }

    public static void ActionBar(params string[] actions)
    {
        Divider();
        Console.WriteLine(string.Join("     ", actions));
    }

    public static void InfoBox(params string[] lines)
    {
        Divider();
        foreach (var line in lines)
            Console.WriteLine($"  \u2139  {line}");
        Divider();
    }

    public static void Success(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(msg.EndsWith('\u2713') ? msg : $"{msg} \u2713");
        Console.ResetColor();
    }

    public static void Error(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    public static void Warning(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\u26a0  {msg}");
        Console.ResetColor();
    }

    public static void Info(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\u2139  {msg}");
        Console.ResetColor();
    }

    public static void AiNote(string msg) =>
        Console.WriteLine($"  Note: {msg}");

    public static string Prompt(string label)
    {
        Console.Write($"{label} : ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static string PromptInline(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static string PromptPassword(string label)
    {
        Console.Write($"{label} : ");
        var pwd = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
            {
                pwd.Remove(pwd.Length - 1, 1);
                Console.Write("\b \b");
            }
            else if (key.Key != ConsoleKey.Backspace)
            {
                pwd.Append(key.KeyChar);
                Console.Write('*');
            }
        }
        Console.WriteLine();
        return pwd.ToString();
    }

    public static string PromptOption(string prompt = "Enter option") =>
        PromptInline(prompt);

    public static void PressAnyKey(string message = "Press any key to continue...")
    {
        Console.WriteLine();
        Console.WriteLine(message);
        Console.ReadKey(intercept: true);
    }

    public static void TableHeader(params string[] cols)
    {
        Console.WriteLine(string.Join("   ", cols));
        Divider();
    }

    public static void TableRow(params string[] cols) =>
        Console.WriteLine(string.Join("   ", cols));

    public static string HealthIcon(string health) => health.ToUpperInvariant().Replace("_", "") switch
    {
        "ATRISK" => "\U0001f534 AT RISK",
        "ATTENTION" or "NEEDSATTENTION" => "\U0001f7e1 ATTENTION",
        _ => "\U0001f7e2 ON TRACK"
    };

    public static string FormatMilestoneStatus(string status, bool isOverdue)
    {
        var formatted = status.ToUpperInvariant() switch
        {
            "NOTSTARTED" => "NOT_STARTED",
            "INPROGRESS" => "IN_PROGRESS",
            "DONE" => "DONE",
            _ when status.Equals("NotStarted", StringComparison.OrdinalIgnoreCase) => "NOT_STARTED",
            _ when status.Equals("InProgress", StringComparison.OrdinalIgnoreCase) => "IN_PROGRESS",
            _ when status.Equals("Done", StringComparison.OrdinalIgnoreCase) => "DONE",
            _ => StatusUpper(status)
        };

        if (isOverdue && formatted != "DONE")
            formatted += "  \u26a0 OVERDUE";

        return formatted;
    }

    public static void RiskFlag(RiskFlagDto flag)
    {
        var symbol = flag.IsPositive ? "\u2713" : "\u2717";
        Console.WriteLine($"  {symbol}  {flag.Message}");
    }

    public static string StatusUpper(string status) =>
        status.Replace(" ", "_").ToUpperInvariant();
}

namespace ProjectManagementSystem.Client.Helpers;

using System.Text;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Manager;

/// <summary>
/// Central rendering helper matching the BRD console layouts in Project_Management.md.
/// Tables auto-size to content (no ellipsis) and use the full console width when needed.
/// </summary>
public static class ConsoleUI
{
    public const int MinContentWidth = 100;
    public const int ColumnGap = 3;

    public static int ContentWidth => GetContentWidth();
    public static int BoxWidth => ContentWidth;

    private const char TL = '\u2554';
    private const char TR = '\u2557';
    private const char BL = '\u255A';
    private const char BR = '\u255D';
    private const char H  = '\u2550';
    private const char V  = '\u2551';
    private const char SH = '\u2500';

    private static int GetContentWidth()
    {
        try
        {
            var width = Console.WindowWidth;
            if (width >= MinContentWidth) return width - 2;
        }
        catch
        {
            // ignored — headless / redirected output
        }

        return MinContentWidth;
    }

    public static string FormatDate(DateOnly date) => date.ToString(UiFormats.DisplayDateShort);

    public static string FormatDate(DateTime date) => date.ToString(UiFormats.DisplayDateShort);

    public static string FormatPercent(int value) => $"{value}%";

    public static string FormatPercent(decimal value) => $"{value}%";

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
        Console.WriteLine(new string(SH, ContentWidth));

    public static void BlankLine() => Console.WriteLine();

    public static void Menu(int number, string label) =>
        Console.WriteLine($"{number}. {label}");

    public static void SubHeader(string title)
    {
        var prefix = $"\u2500\u2500 {title} ";
        var dashes = Math.Max(1, ContentWidth - prefix.Length);
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

    public static void KeyValue(string key, string value, int keyWidth = 15) =>
        Console.WriteLine($"{key.PadRight(keyWidth)}: {value}");

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
        var pwd = new StringBuilder();
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

    /// <summary>
    /// Renders a table sized to fit full cell content (no truncation).
    /// </summary>
    public static void RenderTable(
        string[] headers,
        IEnumerable<string[]> rows,
        int[]? rightAlignColumnIndexes = null)
    {
        var rowList = rows.ToList();
        var colCount = headers.Length;
        var widths = new int[colCount];

        for (var itr = 0; itr < colCount; itr++)
        {
            widths[itr] = headers[itr].Length;
            foreach (var row in rowList)
            {
                if (itr < row.Length)
                    widths[itr] = Math.Max(widths[itr], row[itr].Length);
            }
        }

        var rightAlign = new bool[colCount];
        if (rightAlignColumnIndexes is not null)
        {
            foreach (var idx in rightAlignColumnIndexes)
            {
                if (idx >= 0 && idx < colCount)
                    rightAlign[idx] = true;
            }
        }

        var table = new ConsoleTable(widths, rightAlign, truncate: false);
        table.Header(headers);
        foreach (var row in rowList)
            table.Row(row);
    }

    /// <summary>Standard allocation columns with spaced % and date fields.</summary>
    public static void RenderAllocationTable(
        IEnumerable<(string Col1, string Col2, int Percent, DateOnly From, DateOnly To)> rows,
        string col1Header = "Employee",
        string col2Header = "Project")
    {
        RenderTable(
            [col1Header, col2Header, "%", "From", "To"],
            rows.Select(r => new[]
            {
                r.Col1,
                r.Col2,
                FormatPercent(r.Percent),
                FormatDate(r.From),
                FormatDate(r.To)
            }),
            rightAlignColumnIndexes: [2, 3, 4]);
    }

    /// <summary>Creates a fixed-width column table (prefer <see cref="RenderTable"/> when data is known).</summary>
    public static ConsoleTable Table(params int[] columnWidths) =>
        new(columnWidths, truncate: false);

    /// <summary>Backward-compatible table header — uses default widths for common column counts.</summary>
    public static void TableHeader(params string[] cols)
    {
        var table = new ConsoleTable(ResolveDefaultWidths(cols.Length), truncate: false);
        table.Header(cols);
        _lastTableWidths = table.ColumnWidths;
    }

    /// <summary>Backward-compatible table row — uses widths from the most recent TableHeader call.</summary>
    public static void TableRow(params string[] cols)
    {
        var widths = _lastTableWidths ?? ResolveDefaultWidths(cols.Length);
        var table = new ConsoleTable(widths, truncate: false);
        table.Row(cols);
    }

    private static int[]? _lastTableWidths;

    private static int[] ResolveDefaultWidths(int count) => count switch
    {
        2 => Distribute(20, ContentWidth - 20),
        3 => Distribute(16, 20, ContentWidth - 36),
        4 => Distribute(4, 20, 12, ContentWidth - 36),
        5 => Distribute(3, 22, 22, 5, 11, 11),
        6 => Distribute(3, 18, 18, 5, 11, 11, 10),
        _ => EqualWidths(count)
    };

    private static int[] Distribute(params int[] widths)
    {
        var gap = Math.Max(0, widths.Length - 1) * ColumnGap;
        var total = widths.Sum() + gap;
        if (total < ContentWidth && widths.Length > 0)
        {
            var copy = widths.ToArray();
            copy[^1] += ContentWidth - total;
            return copy;
        }

        return widths;
    }

    private static int[] EqualWidths(int count)
    {
        var gap = Math.Max(0, count - 1) * ColumnGap;
        var baseWidth = Math.Max(6, (ContentWidth - gap) / count);
        var widths = Enumerable.Repeat(baseWidth, count).ToArray();
        widths[^1] += ContentWidth - gap - widths.Sum();
        return widths;
    }

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

public sealed class ConsoleTable
{
    private readonly int[] _widths;
    private readonly bool[] _rightAlign;
    private readonly bool _truncate;

    public ConsoleTable(int[] columnWidths, bool[]? rightAlign = null, bool truncate = false)
    {
        _widths = FitToContentWidth(columnWidths);
        _rightAlign = rightAlign ?? new bool[columnWidths.Length];
        _truncate = truncate;
    }

    private static int[] FitToContentWidth(int[] columnWidths)
    {
        var gap = Math.Max(0, columnWidths.Length - 1) * ConsoleUI.ColumnGap;
        var widths = columnWidths.ToArray();
        var total = widths.Sum() + gap;

        if (total > ConsoleUI.ContentWidth)
            return widths;

        if (total < ConsoleUI.ContentWidth && widths.Length > 0)
            widths[^1] += ConsoleUI.ContentWidth - total;

        return widths;
    }

    public int[] ColumnWidths => _widths;

    public void Header(params string[] columns) => WriteRow(columns);

    public void Row(params string[] columns) => WriteRow(columns);

    private void WriteRow(string[] columns)
    {
        var line = new StringBuilder();
        for (var i = 0; i < _widths.Length; i++)
        {
            if (i > 0)
                line.Append(new string(' ', ConsoleUI.ColumnGap));

            var cell = i < columns.Length ? columns[i] : string.Empty;
            line.Append(FormatCell(cell, _widths[i], i < _rightAlign.Length && _rightAlign[i]));
        }

        Console.WriteLine(line.ToString());
    }

    private string FormatCell(string text, int width, bool rightAlign)
    {
        if (width <= 0) return string.Empty;

        if (_truncate && text.Length > width)
            text = width <= 3 ? text[..width] : text[..(width - 3)] + "...";

        return rightAlign ? text.PadLeft(width) : text.PadRight(width);
    }
}

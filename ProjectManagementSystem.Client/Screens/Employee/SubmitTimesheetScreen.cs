using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.DTOs.Timesheet;

namespace ProjectManagementSystem.Client.Screens.Employee;

/// <summary>Screen 5.1 — Submit Timesheet</summary>
public class SubmitTimesheetScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("SUBMIT TIMESHEET");

        Console.WriteLine("Week Start: Enter date (DD-MM-YYYY) or press Enter for last Monday");
        var weekStr = ConsoleUI.Prompt("Week start");

        string? weekParam = null;
        if (!string.IsNullOrWhiteSpace(weekStr))
        {
            if (!DateOnly.TryParseExact(weekStr, "dd-MM-yyyy", out var week))
            { ConsoleUI.Error("Invalid date format. Use DD-MM-YYYY."); ConsoleUI.PressAnyKey(); return; }
            weekParam = week.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd");
        }

        var (context, error) = await api.GetEmployeeSubmitContextAsync(weekParam);
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (context is null) { ConsoleUI.Error("No data returned."); ConsoleUI.PressAnyKey(); return; }

        Console.Clear();
        ConsoleUI.DrawBox("SUBMIT TIMESHEET");

        ConsoleUI.KeyValue("Employee", context.EmployeeName);
        ConsoleUI.KeyValue("Week", $"{context.WeekStart:dd-MMM-yyyy} - {context.WeekEnd:dd-MMM-yyyy}");

        if (context.AlreadySubmitted)
        {
            ConsoleUI.Warning("A timesheet for this week has already been submitted.");
            ConsoleUI.PressAnyKey();
            return;
        }

        if (context.Allocations.Count == 0)
        {
            ConsoleUI.Warning("You have no active project allocations for this week.");
            ConsoleUI.PressAnyKey();
            return;
        }

        Console.WriteLine("\nChecking your active allocations for this week...");
        ConsoleUI.Divider();

        var entries = new List<SubmitTimesheetProjectEntryDto>();
        var projectIndex = 1;

        foreach (var allocation in context.Allocations)
        {
            Console.WriteLine($"PROJECT {projectIndex} OF {context.Allocations.Count} — {allocation.ProjectName}");
            Console.WriteLine($"  Allocation: {allocation.UtilisationPercent}%   |   Expected: {allocation.MaxHours} hrs max");
            ConsoleUI.Divider();

            var hoursStr = ConsoleUI.Prompt("Hours worked this week");
            if (!decimal.TryParse(hoursStr, out var hours) || hours < 0)
            { ConsoleUI.Error("Invalid hours."); ConsoleUI.PressAnyKey(); return; }

            var tags = PromptActivityTags(context.ActivityTags);
            entries.Add(new SubmitTimesheetProjectEntryDto
            {
                ProjectId = allocation.ProjectId,
                Hours = hours,
                ActivityTags = tags
            });

            projectIndex++;
        }

        var total = entries.Sum(e => e.Hours);
        ConsoleUI.Divider();
        ConsoleUI.SubHeader("SUMMARY");
        ConsoleUI.RenderTable(
            ["Project", "Hours", "Activity Tags"],
            entries.Select(entry =>
            {
                var name = context.Allocations.First(a => a.ProjectId == entry.ProjectId).ProjectName;
                return new[] { name, $"{entry.Hours} hrs", entry.ActivityTags };
            }),
            rightAlignColumnIndexes: [1]);
        ConsoleUI.Divider();
        Console.WriteLine($"  {"Total".PadRight(18)} {($"{total} hrs / {context.MaxWeeklyHours} hrs max").PadLeft(8)}");

        if (total > context.MaxWeeklyHours)
        {
            ConsoleUI.Error($"Total exceeds maximum of {context.MaxWeeklyHours} hrs.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.ActionBar("[S] Submit Timesheet", "[B] Back");
        if (!ConsoleUI.PromptOption().Equals("S", StringComparison.OrdinalIgnoreCase)) return;

        var (result, submitError) = await api.SubmitEmployeeTimesheetAsync(new SubmitEmployeeTimesheetDto
        {
            WeekStartDate = context.WeekStart,
            Projects = entries
        });

        if (submitError is not null) ConsoleUI.Error(submitError);
        else ConsoleUI.Success($"Timesheet submitted successfully. Status: {result!.Status.ToString().ToUpperInvariant()}");
        ConsoleUI.PressAnyKey();
    }

    private static string PromptActivityTags(string[] tags)
    {
        Console.WriteLine("\nWhat did you work on? Select activity tags:");
        for (int i = 0; i < tags.Length; i++)
            Console.WriteLine($"  {i + 1,2}.  {tags[i]}");
        Console.WriteLine($"  {tags.Length + 1,2}.  Other (type manually)");

        var input = ConsoleUI.Prompt("Select tags (comma-separated)");
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var selected = new List<string>();
        foreach (var part in input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var num))
            {
                if (num >= 1 && num <= tags.Length)
                    selected.Add(tags[num - 1]);
                else if (num == tags.Length + 1)
                {
                    var custom = ConsoleUI.Prompt("Enter custom tag");
                    if (!string.IsNullOrWhiteSpace(custom)) selected.Add(custom);
                }
            }
            else
            {
                selected.Add(part);
            }
        }

        return string.Join(", ", selected.Distinct());
    }
}

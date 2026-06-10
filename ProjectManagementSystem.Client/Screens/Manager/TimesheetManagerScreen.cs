using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Client.Screens.Manager;

/// <summary>Screen 4.4 — Team Timesheets</summary>
public class TimesheetManagerScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("TIMESHEETS — MY TEAM");

        Console.WriteLine("Filter by week (DD-MM-YYYY) or press Enter for current week:");
        var weekStr = ConsoleUI.Prompt("Week");
        string? weekParam = null;
        if (!string.IsNullOrWhiteSpace(weekStr))
        {
            if (!DateOnly.TryParseExact(weekStr, UiFormats.DisplayDate, out var week))
            { ConsoleUI.Error("Invalid date format."); ConsoleUI.PressAnyKey(); return; }
            weekParam = week.ToDateTime(TimeOnly.MinValue).ToString(UiFormats.ApiWeekStart);
        }

        var (data, error) = await api.GetManagerTeamTimesheetsAsync(weekParam);
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (data is null) { ConsoleUI.Error("No data returned."); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.Divider();
        var rows = new List<string[]>();
        foreach (var t in data.Submitted)
        {
            var status = t.Status.ToString().ToUpperInvariant();
            if (t.Entries.Count == 0)
            {
                rows.Add([t.EmployeeName, "-", t.TotalHours.ToString(), status]);
                continue;
            }

            foreach (var e in t.Entries)
                rows.Add([t.EmployeeName, e.ProjectName, e.Hours.ToString(), status]);
        }

        foreach (var m in data.Missing)
            rows.Add([m.EmployeeName, m.ProjectName, "0", "MISSED"]);

        ConsoleUI.RenderTable(
            ["Employee", "Project", "Hrs", "Status"],
            rows,
            rightAlignColumnIndexes: [2]);

        ConsoleUI.Divider();
        ConsoleUI.ActionBar("[V] View employee timesheet detail", "[B] Back");
        var opt = ConsoleUI.PromptOption();
        if (opt.Equals("V", StringComparison.OrdinalIgnoreCase))
            await ViewDetailAsync();
    }

    private async Task ViewDetailAsync()
    {
        var idStr = ConsoleUI.Prompt("Timesheet ID");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        Console.Clear();
        ConsoleUI.DrawBox("TIMESHEET DETAIL");

        var (ts, error) = await api.GetManagerTimesheetDetailAsync(id);
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (ts is null) return;

        ConsoleUI.KeyValue("Employee", ts.EmployeeName);
        ConsoleUI.KeyValue("Week", $"{ts.WeekStartDate:dd-MMM-yyyy} - {ts.WeekEndDate:dd-MMM-yyyy}");
        ConsoleUI.KeyValue("Status", ts.Status.ToString().ToUpperInvariant());
        ConsoleUI.KeyValue("Total", $"{ts.TotalHours} hrs");
        ConsoleUI.Divider();

        ConsoleUI.RenderTable(
            ["Project", "Hours", "Activity Tags"],
            ts.Entries.Select(e => new[] { e.ProjectName, e.Hours.ToString(), e.ActivityTags }),
            rightAlignColumnIndexes: [1]);

        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }
}

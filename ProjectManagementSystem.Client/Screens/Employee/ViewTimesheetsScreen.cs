using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;

namespace ProjectManagementSystem.Client.Screens.Employee;

/// <summary>Screen 5.2 — View My Timesheets</summary>
public class ViewTimesheetsScreen(ApiClient api) : IScreen
{
    public async Task ShowAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("MY TIMESHEETS");

        var (timesheets, error) = await api.GetEmployeeTimesheetsAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        var list = timesheets?.ToList() ?? [];
        if (list.Count == 0)
        {
            ConsoleUI.Info("No timesheets found.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.RenderTable(
            ["Week Start", "Total Hrs", "Status"],
            list.Select(t =>
            {
                var statusDisplay = t.Status.ToString().ToUpperInvariant();
                if (t.Status == Core.Enums.TimesheetStatus.Missed)
                    statusDisplay += "  \u26a0";
                return new[]
                {
                    ConsoleUI.FormatDate(t.WeekStartDate),
                    $"{t.TotalHours} hrs",
                    statusDisplay
                };
            }),
            rightAlignColumnIndexes: [1]);

        ConsoleUI.Divider();
        ConsoleUI.ActionBar("[V] View week details", "[B] Back");
        var opt = ConsoleUI.PromptOption();
        if (opt.Equals("V", StringComparison.OrdinalIgnoreCase))
            await ViewDetailAsync();
    }

    private async Task ViewDetailAsync()
    {
        var idStr = ConsoleUI.Prompt("Timesheet ID");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        Console.Clear();

        var (ts, error) = await api.GetEmployeeTimesheetAsync(id);
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (ts is null) return;

        ConsoleUI.SubHeader($"Week: {ts.WeekStartDate:dd-MMM-yyyy} — Status: {ts.Status.ToString().ToUpperInvariant()}");
        ConsoleUI.BlankLine();
        ConsoleUI.RenderTable(
            ["Project", "Hrs", "Activity Tags"],
            ts.Entries.Select(e => new[] { e.ProjectName, e.Hours.ToString(), e.ActivityTags }),
            rightAlignColumnIndexes: [1]);
        ConsoleUI.Divider();
        Console.WriteLine($"Total: {ts.TotalHours} hrs");
        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }
}

using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;

namespace ProjectManagementSystem.Client.Screens.Manager;

/// <summary>Screen 4.1 — Resource Dashboard</summary>
public class ResourceDashboardScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        Console.Clear();
        var monthYear = DateTime.Now.ToString("MMMM yyyy");
        ConsoleUI.DrawBox($"RESOURCE DASHBOARD — {monthYear}");

        var (dashboard, error) = await api.GetManagerDashboardAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (dashboard is null) { ConsoleUI.Error("No data returned."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine($"ON BENCH  ({dashboard.BenchCount} employees available)");
        ConsoleUI.Divider();
        if (dashboard.BenchEmployees.Count == 0)
            Console.WriteLine("(none)");
        else
        {
            ConsoleUI.RenderTable(
                ["ID", "Name", "Department", "Skills"],
                dashboard.BenchEmployees.Select(e => new[]
                {
                    e.EmployeeId.ToString(),
                    e.Name,
                    e.Department,
                    e.Skills
                }),
                rightAlignColumnIndexes: [0]);
        }

        ConsoleUI.BlankLine();
        Console.WriteLine("ACTIVE EMPLOYEES");
        ConsoleUI.Divider();
        if (dashboard.ActiveEmployees.Count == 0)
            Console.WriteLine("(none)");
        else
        {
            ConsoleUI.RenderTable(
                ["ID", "Name", "Alloc %", "Availability"],
                dashboard.ActiveEmployees.Select(e => new[]
                {
                    e.EmployeeId.ToString(),
                    e.Name,
                    ConsoleUI.FormatPercent(e.AllocationPercentage),
                    e.AvailabilityStatus
                }),
                rightAlignColumnIndexes: [0, 2]);
        }

        ConsoleUI.Divider();
        Console.WriteLine($"Bench: {dashboard.BenchCount}   |   Partial: {dashboard.PartialCount}");
        ConsoleUI.ActionBar("[D] Drill into employee details", "[B] Back");

        var opt = ConsoleUI.PromptOption();
        if (opt.Equals("D", StringComparison.OrdinalIgnoreCase))
            await ShowEmployeeDetailAsync();
    }

    private async Task ShowEmployeeDetailAsync()
    {
        var idStr = ConsoleUI.Prompt("Employee ID");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        Console.Clear();
        ConsoleUI.DrawBox("EMPLOYEE DETAIL");

        var (detail, error) = await api.GetManagerEmployeeDetailAsync(id);
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (detail is null) { ConsoleUI.Error("Employee not found."); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.SubHeader(detail.Name);
        ConsoleUI.KeyValue("Department", detail.Department);
        ConsoleUI.KeyValue("Current Status", $"{ConsoleUI.StatusUpper(detail.CurrentStatus)} ({detail.CurrentAllocation}%)");
        ConsoleUI.KeyValue("Profile Skills", detail.Skills);
        ConsoleUI.BlankLine();

        Console.WriteLine("Active Allocations:");
        if (detail.ActiveAllocations.Count == 0)
            Console.WriteLine("  (none)");
        else
        {
            ConsoleUI.RenderTable(
                ["Project", "%", "From", "To"],
                detail.ActiveAllocations.Select(a => new[]
                {
                    a.ProjectName,
                    ConsoleUI.FormatPercent(a.Percentage),
                    ConsoleUI.FormatDate(a.FromDate),
                    ConsoleUI.FormatDate(a.ToDate)
                }),
                rightAlignColumnIndexes: [1, 2, 3]);
        }

        ConsoleUI.BlankLine();
        Console.WriteLine("Recent Activity Tags (last 4 weeks):");
        if (detail.RecentActivityTags.Count == 0)
            Console.WriteLine("  (none)");
        else
            Console.WriteLine($"  {string.Join(", ", detail.RecentActivityTags)}");

        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }
}

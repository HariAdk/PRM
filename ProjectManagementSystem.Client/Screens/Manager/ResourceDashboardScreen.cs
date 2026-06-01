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
            ConsoleUI.TableHeader("ID", "Name", "Department", "Skills");
            foreach (var e in dashboard.BenchEmployees)
                ConsoleUI.TableRow(e.EmployeeId.ToString(), e.Name, e.Department, e.Skills);
        }

        ConsoleUI.BlankLine();
        Console.WriteLine("ACTIVE EMPLOYEES");
        ConsoleUI.Divider();
        if (dashboard.ActiveEmployees.Count == 0)
            Console.WriteLine("(none)");
        else
        {
            ConsoleUI.TableHeader("ID", "Name", "Alloc %", "Availability");
            foreach (var e in dashboard.ActiveEmployees)
                ConsoleUI.TableRow(
                    e.EmployeeId.ToString(),
                    e.Name,
                    $"{e.AllocationPercentage}%",
                    e.AvailabilityStatus);
        }

        ConsoleUI.Divider();
        Console.WriteLine($"Bench: {dashboard.BenchCount}   |   Over-utilised: {dashboard.OverUtilisedCount}   |   Partial: {dashboard.PartialCount}");
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
        Console.WriteLine($"Department     : {detail.Department}");
        Console.WriteLine($"Current Status : {ConsoleUI.StatusUpper(detail.CurrentStatus)} ({detail.CurrentAllocation}%)");
        Console.WriteLine($"Profile Skills : {detail.Skills}");
        ConsoleUI.BlankLine();

        Console.WriteLine("Active Allocations:");
        if (detail.ActiveAllocations.Count == 0)
            Console.WriteLine("  (none)");
        else
        {
            ConsoleUI.TableHeader("Project", "%", "From", "To");
            foreach (var a in detail.ActiveAllocations)
                ConsoleUI.TableRow(
                    a.ProjectName,
                    $"{a.Percentage}%",
                    a.FromDate.ToString("dd-MMM-yy"),
                    a.ToDate.ToString("dd-MMM-yy"));
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

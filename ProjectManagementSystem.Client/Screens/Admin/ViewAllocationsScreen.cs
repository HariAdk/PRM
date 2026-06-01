using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;

namespace ProjectManagementSystem.Client.Screens.Admin;

/// <summary>Screen 3.3 — View All Allocations</summary>
public class ViewAllocationsScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("ALL ALLOCATIONS");

        var (allocations, error) = await api.GetAllocationsAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        var list = allocations?.ToList() ?? [];

        ConsoleUI.TableHeader("Employee", "Project", "%", "From", "To");
        foreach (var a in list)
            ConsoleUI.TableRow(
                a.EmployeeName,
                a.ProjectName,
                $"{a.UtilisationPercent}%",
                a.FromDate.ToString("dd-MMM-yy"),
                a.ToDate.ToString("dd-MMM-yy"));

        ConsoleUI.Divider();
        Console.WriteLine($"Total Active Allocations: {list.Count}");
        ConsoleUI.BlankLine();
        ConsoleUI.ActionBar("[F] Filter by Employee / Project", "[B] Back");

        var opt = ConsoleUI.PromptOption();
        if (opt.ToUpper() == "F") await FilterAsync(list);
    }

    private async Task FilterAsync(List<Core.DTOs.Allocation.AllocationDto> list)
    {
        Console.Clear();
        ConsoleUI.DrawBox("FILTER ALLOCATIONS");
        ConsoleUI.Menu(1, "Filter by Employee name");
        ConsoleUI.Menu(2, "Filter by Project name");
        ConsoleUI.Menu(3, "Back");
        var opt = ConsoleUI.PromptOption();

        IEnumerable<Core.DTOs.Allocation.AllocationDto> filtered = list;
        if (opt == "1")
        {
            var name = ConsoleUI.Prompt("Employee name contains");
            filtered = list.Where(a => a.EmployeeName.Contains(name, StringComparison.OrdinalIgnoreCase));
        }
        else if (opt == "2")
        {
            var name = ConsoleUI.Prompt("Project name contains");
            filtered = list.Where(a => a.ProjectName.Contains(name, StringComparison.OrdinalIgnoreCase));
        }
        else return;

        Console.Clear();
        ConsoleUI.DrawBox("FILTERED ALLOCATIONS");
        ConsoleUI.TableHeader("Employee", "Project", "%", "From", "To");
        foreach (var a in filtered)
            ConsoleUI.TableRow(
                a.EmployeeName,
                a.ProjectName,
                $"{a.UtilisationPercent}%",
                a.FromDate.ToString("dd-MMM-yy"),
                a.ToDate.ToString("dd-MMM-yy"));

        ConsoleUI.PressAnyKey();
    }
}

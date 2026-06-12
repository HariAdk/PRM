using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Manager;

namespace ProjectManagementSystem.Client.Screens.Manager;

/// <summary>Screen 4.2 — Allocate Resource</summary>
public class AllocateResourceScreen(ApiClient api) : IScreen
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("ALLOCATE RESOURCE");

            ConsoleUI.Menu(1, "Find resource using AI (recommended)");
            ConsoleUI.Menu(2, "Allocate directly (I already know who I want)");
            ConsoleUI.Menu(3, "End an existing allocation");
            ConsoleUI.Menu(4, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await AiAllocateAsync(); break;
                case "2": await DirectAllocateAsync(); break;
                case "3": await EndAllocationAsync(); break;
                case "4": return;
                default: ConsoleUI.Error("Invalid option."); ConsoleUI.PressAnyKey(); break;
            }
        }
    }

    private async Task<List<Core.DTOs.Project.ProjectDto>> LoadMyProjectsAsync()
    {
        var (projects, error) = await api.GetManagerProjectsAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return []; }
        return projects?.ToList() ?? [];
    }

    private async Task AiAllocateAsync()
    {
        var projects = await LoadMyProjectsAsync();
        if (projects.Count == 0) return;

        Console.Clear();
        ConsoleUI.DrawBox("ALLOCATE RESOURCE");
        Console.WriteLine("Step 1 — Select Project");
        foreach (var p in projects)
            Console.WriteLine($"  [{p.Id}] {p.Name}");

        var projectIdStr = ConsoleUI.Prompt("Enter project name or ID");
        if (!int.TryParse(projectIdStr, out var projectId)) { ConsoleUI.Error("Invalid project ID."); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.BlankLine();
        Console.WriteLine("Step 2 — Describe your requirement");
        Console.WriteLine("Type what kind of resource you need:");
        var requirement = ConsoleUI.Prompt(">");
        if (string.IsNullOrWhiteSpace(requirement)) { ConsoleUI.Error("Requirement is required."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("\nSearching... (AI matching in progress)");

        var (result, error) = await api.ManagerSkillMatchAsync(new AISkillMatchRequestDto
        {
            Requirement = requirement
        });

        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.Divider();
        if (result?.UsedFallback == true)
        {
            Console.WriteLine("KEYWORD-MATCHED RESULTS (AI unavailable)");
            ConsoleUI.Info(result.FallbackReason ??
                "Showing organization resources whose skills or activity match your requirement.");
        }
        else
            Console.WriteLine("AI-MATCHED RESULTS");
        ConsoleUI.Divider();

        var matches = result?.Matches ?? [];
        if (matches.Count == 0)
        {
            ConsoleUI.Warning(result?.UsedFallback == true
                ? "No organization resources match that requirement. Try different keywords or use direct allocation."
                : "No matching employees found.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.RenderTable(
            ["#", "Name", "Skills Match", "Availability", "Recent Activity"],
            matches.Select((m, i) => new[]
            {
                (i + 1).ToString(),
                m.Name,
                m.SkillsMatch,
                $"{m.AvailabilityPercentage}% free",
                m.RecentActivity
            }),
            rightAlignColumnIndexes: [0]);

        if (result?.UsedFallback != true)
            ConsoleUI.AiNote("Suggestions are AI-generated. Verify before confirming.");
        ConsoleUI.Divider();

        var choice = ConsoleUI.Prompt("Select employee (enter #, or 0 to search again)");
        if (choice == "0") return;
        if (!int.TryParse(choice, out var idx) || idx < 1 || idx > matches.Count)
        { ConsoleUI.Error("Invalid choice."); ConsoleUI.PressAnyKey(); return; }

        await ConfirmAllocationAsync(projectId, matches[idx - 1].EmployeeId, matches[idx - 1].Name);
    }

    private async Task DirectAllocateAsync()
    {
        var projects = await LoadMyProjectsAsync();
        if (projects.Count == 0) return;

        Console.Clear();
        ConsoleUI.DrawBox("DIRECT ALLOCATION");
        foreach (var p in projects)
            Console.WriteLine($"  [{p.Id}] {p.Name}");

        var projectIdStr = ConsoleUI.Prompt("Select Project");
        var employeeIdStr = ConsoleUI.Prompt("Enter Employee ID");
        if (!int.TryParse(projectIdStr, out var projectId) ||
            !int.TryParse(employeeIdStr, out var employeeId))
        { ConsoleUI.Error("Invalid numeric input."); ConsoleUI.PressAnyKey(); return; }

        var (detail, detailErr) = await api.GetManagerEmployeeDetailAsync(employeeId);
        if (detailErr is null && detail is not null)
        {
            ConsoleUI.SubHeader(detail.Name);
            Console.WriteLine($"Current Utilisation: {detail.CurrentAllocation}%");
        }

        await ConfirmAllocationAsync(projectId, employeeId, detail?.Name);
    }

    private async Task ConfirmAllocationAsync(int projectId, int employeeId, string? employeeName = null)
    {
        if (employeeName is not null)
        {
            ConsoleUI.BlankLine();
            Console.WriteLine("Set Allocation:");
        }

        var percentStr = ConsoleUI.Prompt("Utilisation %   ");
        var fromStr = ConsoleUI.Prompt("From Date       (DD-MM-YYYY)");
        var toStr = ConsoleUI.Prompt("To Date         (DD-MM-YYYY)");

        if (!int.TryParse(percentStr, out var percent) ||
            !DateOnly.TryParseExact(fromStr, UiFormats.DisplayDate, out var from) ||
            !DateOnly.TryParseExact(toStr, UiFormats.DisplayDate, out var to))
        { ConsoleUI.Error("Invalid input."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("\nValidating...");
        ConsoleUI.ActionBar("[C] Confirm Allocation", "[B] Back");
        if (!ConsoleUI.PromptOption().Equals("C", StringComparison.OrdinalIgnoreCase)) return;

        var (data, error) = await api.CreateManagerAllocationAsync(new CreateAllocationDto
        {
            ProjectId = projectId,
            EmployeeId = employeeId,
            UtilisationPercent = percent,
            FromDate = from,
            ToDate = to
        });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success($"Allocation saved. ID: {data!.Id}");
        ConsoleUI.PressAnyKey();
    }

    private async Task EndAllocationAsync()
    {
        var projects = await LoadMyProjectsAsync();
        if (projects.Count == 0) return;

        Console.Clear();
        ConsoleUI.DrawBox("END ALLOCATION");

        foreach (var p in projects)
            Console.WriteLine($"  [{p.Id}] {p.Name}");

        var projectIdStr = ConsoleUI.Prompt("Select Project");
        if (!int.TryParse(projectIdStr, out var projectId)) { ConsoleUI.Error("Invalid project ID."); ConsoleUI.PressAnyKey(); return; }

        var (allocations, error) = await api.GetManagerProjectAllocationsAsync(projectId);
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        var list = allocations?.ToList() ?? [];
        if (list.Count == 0)
        {
            ConsoleUI.Info("No active allocations on this project.");
            ConsoleUI.PressAnyKey();
            return;
        }

        Console.WriteLine("\nActive Allocations on this project:");
        ConsoleUI.RenderTable(
            ["#", "Employee", "%", "From", "To"],
            list.Select((a, i) => new[]
            {
                $"{i + 1}.",
                a.EmployeeName,
                ConsoleUI.FormatPercent(a.UtilisationPercent),
                ConsoleUI.FormatDate(a.FromDate),
                ConsoleUI.FormatDate(a.ToDate)
            }),
            rightAlignColumnIndexes: [0, 2, 3, 4]);

        var selStr = ConsoleUI.Prompt("Select allocation to end");
        if (!int.TryParse(selStr, out var sel) || sel < 1 || sel > list.Count)
        { ConsoleUI.Error("Invalid selection."); ConsoleUI.PressAnyKey(); return; }

        var selected = list[sel - 1];
        Console.WriteLine($"\nEnd {selected.EmployeeName}'s allocation on {selected.ProjectName}?");
        Console.WriteLine($"Set end date to today ({DateTime.Today:dd-MMM-yyyy})?");
        ConsoleUI.ActionBar("[Y] Yes, End Now", "[B] Back");
        if (!ConsoleUI.PromptOption().Equals("Y", StringComparison.OrdinalIgnoreCase)) return;

        var (_, error2) = await api.EndManagerAllocationAsync(selected.Id, new EndAllocationDto
        {
            EndDate = DateOnly.FromDateTime(DateTime.Today)
        });
        if (error2 is not null) ConsoleUI.Error(error2);
        else ConsoleUI.Success($"Allocation ended. {selected.EmployeeName} freed from {selected.ProjectName} as of {DateTime.Today:dd-MMM-yyyy}.");
        ConsoleUI.PressAnyKey();
    }
}

using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;

namespace ProjectManagementSystem.Client.Screens.Manager;

/// <summary>Screen 4.3 — My Projects & Health View</summary>
public class MyProjectsScreen(ApiClient api) : IScreen
{
    public async Task ShowAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("MY PROJECTS");

        var (projects, error) = await api.GetManagerProjectsAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        var list = projects?.ToList() ?? [];
        if (list.Count == 0)
        {
            ConsoleUI.Info("No projects assigned to you.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.RenderTable(
            ["#", "Project", "End Date", "Health"],
            list.Select((p, i) => new[]
            {
                $"{i + 1}.",
                p.Name,
                ConsoleUI.FormatDate(p.EndDate),
                ConsoleUI.HealthIcon(p.HealthStatus)
            }),
            rightAlignColumnIndexes: [0]);

        ConsoleUI.Divider();
        var numStr = ConsoleUI.Prompt("Select project number to view details");
        if (!int.TryParse(numStr, out var num) || num < 1 || num > list.Count)
        { ConsoleUI.Error("Invalid selection."); ConsoleUI.PressAnyKey(); return; }

        await ShowProjectDetailAsync(list[num - 1].Id);
    }

    private async Task ShowProjectDetailAsync(int projectId)
    {
        Console.Clear();
        ConsoleUI.DrawBox("MY PROJECTS");

        var (detail, error) = await api.GetManagerProjectDetailAsync(projectId);
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (detail is null) return;

        ConsoleUI.SubHeader(detail.Project.Name);
        ConsoleUI.KeyValue("Health Status", ConsoleUI.HealthIcon(detail.DisplayHealth));
        ConsoleUI.BlankLine();

        Console.WriteLine("Risk Flags:");
        if (detail.RiskFlags.Count == 0)
            Console.WriteLine("  (none)");
        else
            foreach (var flag in detail.RiskFlags)
                ConsoleUI.RiskFlag(flag);

        ConsoleUI.BlankLine();
        Console.WriteLine("Milestones:");
        if (detail.Milestones.Count == 0)
            Console.WriteLine("  (none)");
        else
        {
            ConsoleUI.RenderTable(
                ["#", "Title", "Due Date", "Status"],
                detail.Milestones.Select((m, i) => new[]
                {
                    $"{i + 1}.",
                    m.Title,
                    ConsoleUI.FormatDate(m.DueDate),
                    ConsoleUI.FormatMilestoneStatus(m.Status, m.IsOverdue)
                }),
                rightAlignColumnIndexes: [0]);
        }

        ConsoleUI.BlankLine();
        Console.WriteLine("Allocated Resources:");
        if (detail.Allocations.Count == 0)
            Console.WriteLine("  (none)");
        else
        {
            ConsoleUI.RenderTable(
                ["Name", "%", "From", "To"],
                detail.Allocations.Select(a => new[]
                {
                    a.EmployeeName,
                    ConsoleUI.FormatPercent(a.UtilisationPercent),
                    ConsoleUI.FormatDate(a.FromDate),
                    ConsoleUI.FormatDate(a.ToDate)
                }),
                rightAlignColumnIndexes: [1, 2, 3]);
        }

        ConsoleUI.ActionBar("[A] Get AI Risk Summary", "[B] Back");
        var opt = ConsoleUI.PromptOption();
        if (opt.Equals("A", StringComparison.OrdinalIgnoreCase))
            await ShowRiskSummaryAsync(projectId, detail.Project.Name);
    }

    private async Task ShowRiskSummaryAsync(int projectId, string projectName)
    {
        Console.Clear();
        ConsoleUI.SubHeader($"AI Risk Summary — {projectName}");

        var (result, error) = await api.ManagerRiskSummaryAsync(new Core.DTOs.Manager.AIRiskSummaryRequestDto { ProjectId = projectId });
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.AiNote("This summary is AI-generated from milestone and timesheet data.");
        ConsoleUI.Divider();
        Console.WriteLine($"\n{result?.Summary}");
        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }
}

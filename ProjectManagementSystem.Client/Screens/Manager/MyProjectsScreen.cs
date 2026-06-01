using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.DTOs.Manager;

namespace ProjectManagementSystem.Client.Screens.Manager;

/// <summary>Screen 4.3 — My Projects & Health View</summary>
public class MyProjectsScreen(ApiClient api)
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

        ConsoleUI.TableHeader("#", "Project", "End Date", "Health");
        for (int i = 0; i < list.Count; i++)
        {
            var p = list[i];
            ConsoleUI.TableRow(
                $"{i + 1}.",
                p.Name,
                p.EndDate.ToString("dd-MMM-yy"),
                ConsoleUI.HealthIcon(p.HealthStatus));
        }

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
        Console.WriteLine($"Health Status : {ConsoleUI.HealthIcon(detail.DisplayHealth)}");
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
            ConsoleUI.TableHeader("#", "Title", "Due Date", "Status");
            for (int i = 0; i < detail.Milestones.Count; i++)
            {
                var m = detail.Milestones[i];
                ConsoleUI.TableRow(
                    $"{i + 1}.",
                    m.Title,
                    m.DueDate.ToString("dd-MMM-yy"),
                    ConsoleUI.FormatMilestoneStatus(m.Status, m.IsOverdue));
            }
        }

        ConsoleUI.BlankLine();
        Console.WriteLine("Allocated Resources:");
        if (detail.Allocations.Count == 0)
            Console.WriteLine("  (none)");
        else
        {
            ConsoleUI.TableHeader("Name", "%", "From", "To");
            foreach (var a in detail.Allocations)
                ConsoleUI.TableRow(
                    a.EmployeeName,
                    $"{a.UtilisationPercent}%",
                    a.FromDate.ToString("dd-MMM-yy"),
                    a.ToDate.ToString("dd-MMM-yy"));
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

        var (result, error) = await api.ManagerRiskSummaryAsync(new AIRiskSummaryRequestDto { ProjectId = projectId });
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.AiNote("This summary is AI-generated from milestone and timesheet data.");
        ConsoleUI.Divider();
        Console.WriteLine($"\n{result?.Summary}");
        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }
}

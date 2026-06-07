using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.DTOs.Manager;

namespace ProjectManagementSystem.Client.Screens.Manager;

/// <summary>Screen 4.5 — AI Assistant</summary>
public class AiAssistantScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("AI ASSISTANT");

            ConsoleUI.Menu(1, "Skill Match    — Find best employees for a project requirement");
            ConsoleUI.Menu(2, "Risk Summary   — Get a health analysis for a project");
            ConsoleUI.Menu(3, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await SkillMatchAsync(); break;
                case "2": await RiskSummaryAsync(); break;
                case "3": return;
                default: ConsoleUI.Error("Invalid option."); ConsoleUI.PressAnyKey(); break;
            }
        }
    }

    private async Task<List<Core.DTOs.Project.ProjectDto>> LoadMyProjectsAsync()
    {
        var (projects, error) = await api.GetManagerProjectsAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return []; }
        var list = projects?.ToList() ?? [];
        if (list.Count == 0) ConsoleUI.Info("No projects assigned to you.");
        return list;
    }

    private async Task SkillMatchAsync()
    {
        var projects = await LoadMyProjectsAsync();
        if (projects.Count == 0) { ConsoleUI.PressAnyKey(); return; }

        Console.Clear();
        ConsoleUI.DrawBox("AI ASSISTANT");
        ConsoleUI.SubHeader("Skill Match");
        Console.WriteLine("\nDescribe your project requirement in plain English:");
        var requirement = ConsoleUI.Prompt(">");
        if (string.IsNullOrWhiteSpace(requirement)) { ConsoleUI.Error("Requirement required."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("\nSelect project:");
        for (int i = 0; i < projects.Count; i++)
            Console.WriteLine($"  {i + 1}.  {projects[i].Name}");

        var projectNumStr = ConsoleUI.Prompt("Enter project number");
        if (!int.TryParse(projectNumStr, out var projectNum) || projectNum < 1 || projectNum > projects.Count)
        { ConsoleUI.Error("Invalid project."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("\nSearching... (calling AI)");

        var (result, error) = await api.ManagerSkillMatchAsync(new AISkillMatchRequestDto
        {
            ProjectId = projects[projectNum - 1].Id,
            Requirement = requirement
        });

        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("\nResults:");
        var matches = result?.Matches ?? [];
        if (matches.Count == 0)
            ConsoleUI.Warning("No matches found.");
        else
        {
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

            for (int i = 0; i < matches.Count; i++)
            {
                Console.WriteLine($"      Reason: {matches[i].Reason}");
                ConsoleUI.BlankLine();
            }
        }

        ConsoleUI.Info("These are AI-generated suggestions. Always verify availability and skills with the employee before allocating.");
        ConsoleUI.ActionBar("[A] Go to Allocate Resource", "[B] Back");
        ConsoleUI.PromptOption();
    }

    private async Task RiskSummaryAsync()
    {
        var projects = await LoadMyProjectsAsync();
        if (projects.Count == 0) { ConsoleUI.PressAnyKey(); return; }

        Console.Clear();
        ConsoleUI.DrawBox("AI ASSISTANT");
        ConsoleUI.SubHeader("Risk Summary");
        Console.WriteLine("\nSelect project:");
        for (int i = 0; i < projects.Count; i++)
            Console.WriteLine($"  {i + 1}.  {projects[i].Name}    {ConsoleUI.HealthIcon(projects[i].HealthStatus)}");

        var projectNumStr = ConsoleUI.Prompt("Enter project number");
        if (!int.TryParse(projectNumStr, out var projectNum) || projectNum < 1 || projectNum > projects.Count)
        { ConsoleUI.Error("Invalid project."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("\nGenerating AI summary...");

        var (result, error) = await api.ManagerRiskSummaryAsync(new AIRiskSummaryRequestDto
        {
            ProjectId = projects[projectNum - 1].Id
        });
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.AiNote("AI-generated from current milestone and timesheet data.");
        ConsoleUI.Divider();
        Console.WriteLine($"\n{result?.Summary}");
        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }
}

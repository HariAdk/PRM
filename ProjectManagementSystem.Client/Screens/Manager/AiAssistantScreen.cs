using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Core.DTOs.Manager;
namespace ProjectManagementSystem.Client.Screens.Manager;


public class AiAssistantScreen(ApiClient api) : IScreen
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("AI ASSISTANT");
            ConsoleUI.Menu(1, "Skill Match         — Find best employees org-wide for a requirement");
            ConsoleUI.Menu(2, "Complete Team Build — Assemble a team from bench resources org-wide");
            ConsoleUI.Menu(3, "Risk Summary        — Get a health analysis for a project");
            ConsoleUI.Menu(4, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await SkillMatchAsync(); break;
                case "2": await TeamBuildAsync(); break;
                case "3": await RiskSummaryAsync(); break;
                case "4": return;
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

        Console.Clear();
        ConsoleUI.DrawBox("AI ASSISTANT");
        ConsoleUI.SubHeader("Skill Match");

        Console.WriteLine("\nDescribe your resource requirement in plain English:");

        Console.WriteLine("(Searches all employees in the organization)");

        var requirement = ConsoleUI.Prompt(">");

        if (string.IsNullOrWhiteSpace(requirement))
        {
            ConsoleUI.Error("Requirement required.");
            ConsoleUI.PressAnyKey();
            return;
        }

        Console.WriteLine("\nSearching... (calling AI)");

        var (result, error) = await api.ManagerSkillMatchAsync(new AISkillMatchRequestDto
        {
            Requirement = requirement
        });

        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (result?.UsedFallback == true)
        {
            ConsoleUI.Info(result.FallbackReason ??
                "AI unavailable — showing keyword matches from the organization.");
            Console.WriteLine("\nKEYWORD-MATCHED RESULTS (AI was not used):");
        }
        else
            Console.WriteLine("\nAI-MATCHED RESULTS:");

        Console.WriteLine();

        var matches = result?.Matches ?? [];

        if (matches.Count == 0)

            ConsoleUI.Warning(result?.UsedFallback == true

                ? "No organization resources match that requirement."

                : "No matches found.");

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

        ConsoleUI.Info(result?.UsedFallback == true
            ? "These matches are keyword-based. Fix LLM settings in Admin → System Configuration for AI suggestions."
            : "These are AI-generated suggestions. Always verify availability and skills with the employee before allocating.");

        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }

    private async Task TeamBuildAsync()

    {
        Console.Clear();
        ConsoleUI.DrawBox("AI ASSISTANT");
        ConsoleUI.SubHeader("Complete Team Building");
        Console.WriteLine("\nDescribe the team you need in plain English.");
        Console.WriteLine("Example: I need 1 JAVA developer, 1 QA, 1 SDET, 1 DevOps engineer");
        Console.WriteLine("(Searches BENCH employees across the entire organization)");
        var requirement = ConsoleUI.Prompt(">");

        if (string.IsNullOrWhiteSpace(requirement))
        {
            ConsoleUI.Error("Team requirement is required.");
            ConsoleUI.PressAnyKey();
            return;
        }

        Console.WriteLine("\nBuilding team... (calling AI)");

        var (result, error) = await api.ManagerTeamBuildAsync(new AITeamBuildRequestDto
        {
            Requirement = requirement
        });



        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }



        if (result?.UsedFallback == true)
        {
            ConsoleUI.Info(result.FallbackReason ??
                "AI unavailable — showing rule-based bench matches from the organization.");
            Console.WriteLine("\nKEYWORD-BASED SUGGESTED TEAM (AI was not used):");
        }
        else
            Console.WriteLine("\nAI-SUGGESTED TEAM:");

        var roles = result?.Roles ?? [];

        if (roles.Count == 0)
        {
            ConsoleUI.Warning("No roles could be parsed from your requirement.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.RenderTable(
            ["Role", "Employee", "Skills (Proficiency)", "Status"],
            roles.Select(r => new[]
            {
                r.Role,
                string.IsNullOrWhiteSpace(r.EmployeeName) ? "—" : r.EmployeeName,
                string.IsNullOrWhiteSpace(r.SkillsMatch) ? "—" : r.SkillsMatch,
                r.Status
            }));

        ConsoleUI.BlankLine();
        foreach (var role in roles)
        {
            var statusLabel = role.Status.Equals("Matched", StringComparison.OrdinalIgnoreCase)
                ? "MATCHED"
                : "NOT FOUND";

            Console.WriteLine($"  {role.Role}  [{statusLabel}]");
            Console.WriteLine($"      {role.Reason}");
            ConsoleUI.BlankLine();
        }

        ConsoleUI.Info(result?.UsedFallback == true
            ? "These matches are keyword-based. Fix LLM settings in Admin → System Configuration for AI suggestions."
            : "These are AI-generated suggestions. Verify skills and bench status before allocating.");
        ConsoleUI.ActionBar("[B] Back");
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
        {
            ConsoleUI.Error("Invalid project.");
            ConsoleUI.PressAnyKey(); return;
        }
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

using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.DTOs.Project;

namespace ProjectManagementSystem.Client.Screens.Admin;

/// <summary>Screen 3.2 — Manage Projects</summary>
public class ManageProjectsScreen(ApiClient api)
{
    private static readonly string[] Statuses = ["Planned", "Active", "OnHold", "Completed"];

    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("MANAGE PROJECTS");

            ConsoleUI.Menu(1, "Create Project");
            ConsoleUI.Menu(2, "View All Projects");
            ConsoleUI.Menu(3, "Update Project Details");
            ConsoleUI.Menu(4, "Manage Milestones");
            ConsoleUI.Menu(5, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await CreateProjectAsync();     break;
                case "2": await ViewAllProjectsAsync();   break;
                case "3": await UpdateProjectAsync();     break;
                case "4": await ManageMilestonesAsync();  break;
                case "5": return;
                default:  ConsoleUI.Error("Invalid option."); ConsoleUI.PressAnyKey(); break;
            }
        }
    }

    private async Task CreateProjectAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("CREATE PROJECT");

        var name      = ConsoleUI.Prompt("Project Name  ");
        var desc      = ConsoleUI.Prompt("Description   ");
        var startStr  = ConsoleUI.Prompt("Start Date    (DD-MM-YYYY)");
        var endStr    = ConsoleUI.Prompt("End Date      (DD-MM-YYYY)");
        Console.WriteLine("Status        : (1) PLANNED   (2) ACTIVE   (3) ON_HOLD");
        var statusChoice = ConsoleUI.Prompt("Enter choice");
        var managerIdStr = ConsoleUI.Prompt("Assign Manager (Enter Manager ID)");
        var spStr        = ConsoleUI.Prompt("Total Story Points");

        ConsoleUI.ActionBar("[S] Save", "[B] Back");
        if (ConsoleUI.PromptOption().ToUpper() == "B") return;

        if (!DateOnly.TryParseExact(startStr, "dd-MM-yyyy", out var start) ||
            !DateOnly.TryParseExact(endStr,   "dd-MM-yyyy", out var end))
        { ConsoleUI.Error("Invalid date format. Use DD-MM-YYYY."); ConsoleUI.PressAnyKey(); return; }

        if (!int.TryParse(managerIdStr, out var managerId) ||
            !int.TryParse(statusChoice, out var sc) || sc < 1 || sc > 3 ||
            !int.TryParse(spStr, out var totalSp) || totalSp < 0)
        { ConsoleUI.Error("Invalid input."); ConsoleUI.PressAnyKey(); return; }

        var (data, error) = await api.CreateProjectAsync(new CreateProjectDto
        {
            Name             = name,
            Description      = desc,
            StartDate        = start,
            EndDate          = end,
            Status           = Statuses[sc - 1],
            ManagerId        = managerId,
            TotalStoryPoints = totalSp
        });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success($"Project created. ID: {data!.Id}");
        ConsoleUI.PressAnyKey();
    }

    private async Task ViewAllProjectsAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("ALL PROJECTS");

        var (projects, error) = await api.GetProjectsAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        var list = projects?.ToList() ?? [];
        ConsoleUI.RenderTable(
            ["ID", "Name", "Manager", "End Date", "Status", "SP Done/Total"],
            list.Select(p => new[]
            {
                p.Id.ToString(),
                p.Name,
                p.ManagerName,
                ConsoleUI.FormatDate(p.EndDate),
                ConsoleUI.StatusUpper(p.Status),
                $"{p.CompletedStoryPoints} / {p.TotalStoryPoints}"
            }),
            rightAlignColumnIndexes: [0]);

        ConsoleUI.Divider();
        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }

    private async Task UpdateProjectAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("UPDATE PROJECT DETAILS");

        var idStr = ConsoleUI.Prompt("Enter Project ID");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        var (proj, err) = await api.GetProjectAsync(id);
        if (err is not null) { ConsoleUI.Error(err); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.BlankLine();
        Console.WriteLine($"Current: {proj!.Name} | Manager: {proj.ManagerName} | Status: {proj.Status}");
        ConsoleUI.Divider();

        var name      = ConsoleUI.Prompt("Name         (Enter to keep)");
        var desc      = ConsoleUI.Prompt("Description  (Enter to keep)");
        var startStr  = ConsoleUI.Prompt("Start Date DD-MM-YYYY (Enter to keep)");
        var endStr    = ConsoleUI.Prompt("End Date   DD-MM-YYYY (Enter to keep)");

        Console.WriteLine("Status        : (1) PLANNED   (2) ACTIVE   (3) ON_HOLD   (4) COMPLETED");
        var sc  = ConsoleUI.Prompt("Enter choice (Enter to keep)");
        var mgr = ConsoleUI.Prompt("Manager ID   (Enter to keep)");
        var sp  = ConsoleUI.Prompt($"Total Story Points (current: {proj.TotalStoryPoints}, Enter to keep)");

        ConsoleUI.ActionBar("[S] Save", "[B] Back");
        if (ConsoleUI.PromptOption().ToUpper() == "B") return;

        var start = string.IsNullOrEmpty(startStr) ? proj.StartDate
                    : DateOnly.TryParseExact(startStr, "dd-MM-yyyy", out var s) ? s : proj.StartDate;
        var end   = string.IsNullOrEmpty(endStr) ? proj.EndDate
                    : DateOnly.TryParseExact(endStr,   "dd-MM-yyyy", out var e) ? e : proj.EndDate;
        var status    = (!string.IsNullOrEmpty(sc) && int.TryParse(sc, out var si) && si >= 1 && si <= Statuses.Length)
                        ? Statuses[si - 1] : proj.Status;
        var managerId = (!string.IsNullOrEmpty(mgr) && int.TryParse(mgr, out var mi)) ? mi : proj.ManagerId;
        var totalSp   = (!string.IsNullOrEmpty(sp) && int.TryParse(sp, out var tsp) && tsp >= 0)
                        ? tsp : proj.TotalStoryPoints;

        var (_, error) = await api.UpdateProjectAsync(id, new UpdateProjectDto
        {
            Name             = string.IsNullOrEmpty(name) ? proj.Name        : name,
            Description      = string.IsNullOrEmpty(desc) ? proj.Description : desc,
            StartDate        = start,
            EndDate          = end,
            Status           = status,
            ManagerId        = managerId,
            TotalStoryPoints = totalSp
        });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("Project updated.");
        ConsoleUI.PressAnyKey();
    }

    private async Task ManageMilestonesAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("MILESTONES");

        var idStr = ConsoleUI.Prompt("Enter Project ID");
        if (!int.TryParse(idStr, out var projectId)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        var (proj, projErr) = await api.GetProjectAsync(projectId);
        if (projErr is not null) { ConsoleUI.Error(projErr); ConsoleUI.PressAnyKey(); return; }

        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("MILESTONES");
            ConsoleUI.SubHeader(proj!.Name);

            var (milestones, err) = await api.GetMilestonesAsync(projectId);
            if (err is not null) { ConsoleUI.Error(err); ConsoleUI.PressAnyKey(); return; }

            var list = milestones?.ToList() ?? [];
            ConsoleUI.RenderTable(
                ["#", "Title", "Due Date", "Story Pts", "Status"],
                list.Select((m, i) =>
                {
                    var overdue = m.Status == "InProgress" && m.DueDate < DateOnly.FromDateTime(DateTime.Today) ? "  OVERDUE" : "";
                    return new[]
                    {
                        $"{i + 1}.",
                        m.Title,
                        ConsoleUI.FormatDate(m.DueDate),
                        m.StoryPoints.ToString(),
                        ConsoleUI.StatusUpper(m.Status) + overdue
                    };
                }),
                rightAlignColumnIndexes: [0, 3]);

            var completedSp = list.Where(m => m.Status.Equals("Done", StringComparison.OrdinalIgnoreCase)).Sum(m => m.StoryPoints);
            var totalSp     = list.Sum(m => m.StoryPoints);
            var remainingSp = totalSp - completedSp;

            ConsoleUI.Divider();
            Console.WriteLine($"Total: {totalSp} SP   |   Completed: {completedSp} SP   |   Remaining: {remainingSp} SP");
            ConsoleUI.BlankLine();
            ConsoleUI.Menu(1, "Add Milestone");
            ConsoleUI.Menu(2, "Update Milestone Status");
            ConsoleUI.Menu(3, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await AddMilestoneAsync(projectId);          break;
                case "2": await UpdateMilestoneStatusAsync(projectId, list); break;
                case "3": return;
                default:  ConsoleUI.Error("Invalid option."); ConsoleUI.PressAnyKey(); break;
            }
        }
    }

    private async Task AddMilestoneAsync(int projectId)
    {
        ConsoleUI.BlankLine();
        var title   = ConsoleUI.Prompt("Milestone Title");
        var dueStr  = ConsoleUI.Prompt("Due Date (DD-MM-YYYY)");
        var spStr   = ConsoleUI.Prompt("Story Points");

        if (!DateOnly.TryParseExact(dueStr, "dd-MM-yyyy", out var due))
        { ConsoleUI.Error("Invalid date format."); ConsoleUI.PressAnyKey(); return; }

        if (!int.TryParse(spStr, out var storyPoints) || storyPoints < 0)
        { ConsoleUI.Error("Story points must be a non-negative number."); ConsoleUI.PressAnyKey(); return; }

        var (_, error) = await api.AddMilestoneAsync(projectId, new CreateMilestoneDto
        {
            Title       = title,
            DueDate     = due,
            StoryPoints = storyPoints
        });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("Milestone added.");
        ConsoleUI.PressAnyKey();
    }

    private async Task UpdateMilestoneStatusAsync(int projectId, List<MilestoneDto> list)
    {
        if (list.Count == 0) { ConsoleUI.Warning("No milestones."); ConsoleUI.PressAnyKey(); return; }

        var idxStr = ConsoleUI.Prompt("Enter milestone number");
        if (!int.TryParse(idxStr, out var idx) || idx < 1 || idx > list.Count)
        { ConsoleUI.Error("Invalid selection."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("\nNew Status:");
        Console.WriteLine("  (1) NotStarted");
        Console.WriteLine("  (2) InProgress");
        Console.WriteLine("  (3) Done");
        var sc = ConsoleUI.Prompt("Enter choice");
        var statusMap = new[] { "NotStarted", "InProgress", "Done" };
        if (!int.TryParse(sc, out var si) || si < 1 || si > 3)
        { ConsoleUI.Error("Invalid status."); ConsoleUI.PressAnyKey(); return; }

        var (_, error) = await api.UpdateMilestoneStatusAsync(projectId, list[idx - 1].Id,
            new UpdateMilestoneStatusDto { Status = statusMap[si - 1] });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("Milestone status updated.");
        ConsoleUI.PressAnyKey();
    }
}

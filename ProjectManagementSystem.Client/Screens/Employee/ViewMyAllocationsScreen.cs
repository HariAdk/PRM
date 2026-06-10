using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;

namespace ProjectManagementSystem.Client.Screens.Employee;

/// <summary>Screen 5.3 — View My Allocations</summary>
public class ViewMyAllocationsScreen(ApiClient api) : IScreen
{
    public async Task ShowAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("MY ALLOCATIONS");

        var (profile, error) = await api.GetEmployeeAllocationsAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }
        if (profile is null) { ConsoleUI.Error("No data returned."); ConsoleUI.PressAnyKey(); return; }

        var allocations = profile.Allocations.ToList();
        if (allocations.Count == 0)
        {
            ConsoleUI.Info("No allocations found.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.RenderTable(
            ["Project", "%", "From", "To", "Status"],
            allocations.Select(a => new[]
            {
                a.ProjectName,
                ConsoleUI.FormatPercent(a.UtilisationPercent),
                ConsoleUI.FormatDate(a.FromDate),
                ConsoleUI.FormatDate(a.ToDate),
                a.IsActive ? "ACTIVE" : "ENDED"
            }),
            rightAlignColumnIndexes: [1, 2, 3]);

        ConsoleUI.Divider();
        Console.WriteLine($"Total Utilisation: {profile.TotalUtilisation}%");
        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }
}

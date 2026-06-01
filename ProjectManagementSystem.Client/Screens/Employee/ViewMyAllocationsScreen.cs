using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;

namespace ProjectManagementSystem.Client.Screens.Employee;

/// <summary>Screen 5.3 — View My Allocations</summary>
public class ViewMyAllocationsScreen(ApiClient api)
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

        ConsoleUI.TableHeader("Project", "%", "From", "To", "Status");
        foreach (var a in allocations)
        {
            var status = a.IsActive ? "ACTIVE" : "ENDED";
            ConsoleUI.TableRow(
                a.ProjectName,
                $"{a.UtilisationPercent}%",
                a.FromDate.ToString("dd-MMM-yy"),
                a.ToDate.ToString("dd-MMM-yy"),
                status);
        }

        ConsoleUI.Divider();
        Console.WriteLine($"Total Utilisation: {profile.TotalUtilisation}%");
        ConsoleUI.ActionBar("[B] Back");
        ConsoleUI.PromptOption();
    }
}

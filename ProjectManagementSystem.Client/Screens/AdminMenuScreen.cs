using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Client.Screens.Admin;

namespace ProjectManagementSystem.Client.Screens;

/// <summary>
/// Screen 3 � Admin Panel Main Menu
/// </summary>
public class AdminMenuScreen(ApiClient api, SessionContext session)
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            var now = DateTime.Now.ToString("dd-MM-yyyy  HH:mm");
            ConsoleUI.DrawBox("ADMIN PANEL", $"Welcome, {session.FullName}  |  {now}");

            ConsoleUI.Menu(1, "Manage Employees");
            ConsoleUI.Menu(2, "Manage Projects");
            ConsoleUI.Menu(3, "View All Allocations");
            ConsoleUI.Menu(4, "Manage Users");
            ConsoleUI.Menu(5, "System Configuration");
            ConsoleUI.Menu(6, "Logout");

            var opt = ConsoleUI.PromptOption();

            switch (opt)
            {
                case "1": await new ManageEmployeesScreen(api).ShowAsync();   break;
                case "2": await new ManageProjectsScreen(api).ShowAsync();    break;
                case "3": await new ViewAllocationsScreen(api).ShowAsync();   break;
                case "4": await new ManageUsersScreen(api).ShowAsync();       break;
                case "5": await new SystemConfigScreen(api).ShowAsync();      break;
                case "6":
                    session.Clear();
                    api.ClearToken();
                    return;
                default:
                    ConsoleUI.Error("Invalid option.");
                    ConsoleUI.PressAnyKey();
                    break;
            }
        }
    }
}

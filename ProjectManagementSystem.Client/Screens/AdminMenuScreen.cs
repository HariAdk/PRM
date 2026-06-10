using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Client.Screens;

public class AdminMenuScreen(ApiClient api, SessionContext session, IScreenFactory screenFactory) : IScreen
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            var currentDate = DateTime.Now.ToString(UiFormats.DisplayDateTime);
            ConsoleUI.DrawBox("ADMIN PANEL", $"Welcome, {session.FullName}  |  {currentDate}");

            ConsoleUI.Menu((int)AdminMenuAction.ManageEmployees, "Manage Employees");
            ConsoleUI.Menu((int)AdminMenuAction.ManageProjects, "Manage Projects");
            ConsoleUI.Menu((int)AdminMenuAction.ViewAllocations, "View All Allocations");
            ConsoleUI.Menu((int)AdminMenuAction.ManageUsers, "Manage Users");
            ConsoleUI.Menu((int)AdminMenuAction.SystemConfiguration, "System Configuration");
            ConsoleUI.Menu((int)AdminMenuAction.Logout, "Logout");

            if (!MenuOptionParser.TryParse(ConsoleUI.PromptOption(), out AdminMenuAction action))
            {
                ConsoleUI.Error("Invalid option.");
                ConsoleUI.PressAnyKey();
                continue;
            }

            if (action == AdminMenuAction.Logout)
            {
                session.Clear();
                api.ClearToken();
                return;
            }

            await screenFactory.CreateAdminScreen(action).ShowAsync();
        }
    }
}

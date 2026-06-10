using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Client.Screens;

/// <summary>Screen 4 — Manager Menu</summary>
public class ManagerMenuScreen(ApiClient api, SessionContext session, IScreenFactory screenFactory) : IScreen
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            var now = DateTime.Now.ToString(UiFormats.DisplayDateTimeShort);
            ConsoleUI.DrawBox($"Welcome, {session.FullName}!  |  {now}");

            ConsoleUI.Menu((int)ManagerMenuAction.ResourceDashboard, "Resource Dashboard");
            ConsoleUI.Menu((int)ManagerMenuAction.AllocateResource, "Allocate Resource");
            ConsoleUI.Menu((int)ManagerMenuAction.MyProjects, "My Projects");
            ConsoleUI.Menu((int)ManagerMenuAction.Timesheets, "Timesheets");
            ConsoleUI.Menu((int)ManagerMenuAction.AiAssistant, "AI Assistant");
            ConsoleUI.Menu((int)ManagerMenuAction.Logout, "Logout");

            if (!MenuOptionParser.TryParse(ConsoleUI.PromptOption(), out ManagerMenuAction action))
            {
                ConsoleUI.Error("Invalid option.");
                ConsoleUI.PressAnyKey();
                continue;
            }

            if (action == ManagerMenuAction.Logout)
            {
                session.Clear();
                api.ClearToken();
                return;
            }

            await screenFactory.CreateManagerScreen(action).ShowAsync();
        }
    }
}

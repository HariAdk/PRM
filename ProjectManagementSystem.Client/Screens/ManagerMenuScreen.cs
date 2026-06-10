using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Client.Screens.Manager;
using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Client.Screens;

/// <summary>Screen 4 � Manager Menu</summary>
public class ManagerMenuScreen(ApiClient api, SessionContext session)
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            var now = DateTime.Now.ToString(UiFormats.DisplayDateTimeShort);
            ConsoleUI.DrawBox($"Welcome, {session.FullName}!  |  {now}");

            ConsoleUI.Menu(1, "Resource Dashboard");
            ConsoleUI.Menu(2, "Allocate Resource");
            ConsoleUI.Menu(3, "My Projects");
            ConsoleUI.Menu(4, "Timesheets");
            ConsoleUI.Menu(5, "AI Assistant");
            ConsoleUI.Menu(6, "Logout");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await new ResourceDashboardScreen(api).ShowAsync(); break;
                case "2": await new AllocateResourceScreen(api).ShowAsync(); break;
                case "3": await new MyProjectsScreen(api).ShowAsync(); break;
                case "4": await new TimesheetManagerScreen(api).ShowAsync(); break;
                case "5": await new AiAssistantScreen(api).ShowAsync(); break;
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

using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Client.Screens;

namespace ProjectManagementSystem.Client;

/// <summary>
/// Routes the logged-in user to the correct role-based menu.
/// Reads the role from SessionContext and delegates to the appropriate screen.
/// </summary>
public class ScreenRouter(ApiClient api, SessionContext session)
{
    public async Task RouteAsync()
    {
        switch (session.Role.ToUpper())
        {
            case "ADMIN":
                await new AdminMenuScreen(api, session).ShowAsync();
                break;
            case "MANAGER":
                await new ManagerMenuScreen(api, session).ShowAsync();
                break;
            case "EMPLOYEE":
                await new EmployeeMenuScreen(api, session).ShowAsync();
                break;
            default:
                ConsoleUI.Error($"Unknown role: {session.Role}. Contact system administrator.");
                ConsoleUI.PressAnyKey();
                break;
        }
    }
}

using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Client.Session;

namespace ProjectManagementSystem.Client;

public class ScreenRouter(IScreenFactory screenFactory, SessionContext session)
{
    public async Task RouteAsync()
    {
        try
        {
            var menu = screenFactory.CreateRoleMenu(session.Role);
            await menu.ShowAsync();
        }
        catch (ArgumentOutOfRangeException)
        {
            ConsoleUI.Error($"Unknown role: {session.Role}. Contact system administrator.");
            ConsoleUI.PressAnyKey();
        }
    }
}

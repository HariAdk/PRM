using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Client.Session;

namespace ProjectManagementSystem.Client;

/// <summary>
/// Routes the logged-in user to the correct role-based menu via <see cref="IScreenFactory"/>.
/// </summary>
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

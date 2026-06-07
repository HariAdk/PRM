using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Core.DTOs.Auth;

namespace ProjectManagementSystem.Client.Screens;

/// <summary>
/// Screen 1 � Application Start / Login
/// ????????????????????????????????????????????????
/// ?    PROJECT & RESOURCE MANAGEMENT TOOL        ?
/// ?    Learn & Code � Final Project              ?
/// ????????????????????????????????????????????????
/// </summary>
public class StartScreen(ApiClient api, SessionContext session)
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("PROJECT & RESOURCE MANAGEMENT TOOL", "Learn & Code \u2014 Final Project");

            ConsoleUI.Menu(1, "Login");
            ConsoleUI.Menu(2, "Sign Up");
            ConsoleUI.Menu(3, "Exit");

            var opt = ConsoleUI.PromptOption();

            switch (opt)
            {
                case "1":
                    await HandleLoginAsync();
                    if (session.IsLoggedIn) return;   // hand off to ScreenRouter
                    break;
                case "2":
                    await new SignUpScreen(api).ShowAsync();
                    break;
                case "3":
                    Console.Clear();
                    Console.WriteLine("\nGoodbye!\n");
                    Environment.Exit(0);
                    break;
                default:
                    ConsoleUI.Error("Invalid option. Please enter 1, 2 or 3.");
                    ConsoleUI.PressAnyKey();
                    break;
            }
        }
    }

    private async Task HandleLoginAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("LOGIN");

        var username = ConsoleUI.Prompt("Username");
        var password = ConsoleUI.PromptPassword("Password");

        Console.WriteLine("\nLogging in...");

        var (data, error) = await api.LoginAsync(new LoginRequestDto
        {
            Username = username,
            Password = password
        });

        if (error is not null || data is null)
        {
            ConsoleUI.Error(error ?? "Login failed.");
            ConsoleUI.PressAnyKey();
            return;
        }

        session.Set(data.UserId, data.FullName, data.Role, data.Token, data.ForcePasswordChange);
        api.SetToken(data.Token);

        // Force password change � cannot be skipped
        if (session.ForcePasswordChange)
        {
            var changed = await new ChangePasswordScreen(api, session).ShowAsync();
            if (!changed)
            {
                session.Clear();
                api.ClearToken();
            }
        }
    }
}

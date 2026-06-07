using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.DTOs.Auth;

namespace ProjectManagementSystem.Client.Screens;

/// <summary>
/// Screen 2 � Sign Up (Manager / Employee only)
/// </summary>
public class SignUpScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("SIGN UP");

        var fullName = ConsoleUI.Prompt("Full Name   ");
        var email    = ConsoleUI.Prompt("Email       ");
        var username = ConsoleUI.Prompt("Username    ");
        var password = ConsoleUI.PromptPassword("Password    ");

        ConsoleUI.BlankLine();
        Console.WriteLine("Role        : (1) Manager   (2) Employee");
        ConsoleUI.InfoBox(
            "Admin accounts can only be",
            "created by an existing Admin from inside",
            "the application.");

        var roleOpt = ConsoleUI.Prompt("Enter choice");
        var role = roleOpt switch
        {
            "1" => "Manager",
            "2" => "Employee",
            _   => string.Empty
        };

        if (string.IsNullOrEmpty(role))
        {
            ConsoleUI.Error("Invalid role selection.");
            ConsoleUI.PressAnyKey();
            return;
        }

        ConsoleUI.ActionBar("[S] Submit", "[B] Back");
        var opt = ConsoleUI.PromptOption();

        if (opt.ToUpper() == "B") return;
        if (opt.ToUpper() != "S")
        {
            ConsoleUI.Error("Invalid option.");
            ConsoleUI.PressAnyKey();
            return;
        }

        var (_, error) = await api.SignUpAsync(new SignUpRequestDto
        {
            FullName = fullName,
            Email    = email,
            Username = username,
            Password = password,
            Role     = role
        });

        if (error is not null)
        {
            ConsoleUI.Error(error);
        }
        else
        {
            ConsoleUI.Success("Account created. Please log in.");
        }
        ConsoleUI.PressAnyKey();
    }
}

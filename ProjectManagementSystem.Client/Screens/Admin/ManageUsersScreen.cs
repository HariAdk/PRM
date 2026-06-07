using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.DTOs.User;

namespace ProjectManagementSystem.Client.Screens.Admin;

/// <summary>Screen 3.4 � Manage Users</summary>
public class ManageUsersScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("MANAGE USERS");

            ConsoleUI.Menu(1, "Create User Account");
            ConsoleUI.Menu(2, "View All Users");
            ConsoleUI.Menu(3, "Reset User Password");
            ConsoleUI.Menu(4, "Deactivate User");
            ConsoleUI.Menu(5, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await CreateUserAsync();     break;
                case "2": await ViewAllUsersAsync();   break;
                case "3": await ResetPasswordAsync();  break;
                case "4": await DeactivateUserAsync(); break;
                case "5": return;
                default:  ConsoleUI.Error("Invalid option."); ConsoleUI.PressAnyKey(); break;
            }
        }
    }

    private async Task CreateUserAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("CREATE USER ACCOUNT");

        var fullName  = ConsoleUI.Prompt("Full Name         ");
        var email     = ConsoleUI.Prompt("Email             ");
        var username  = ConsoleUI.Prompt("Username          ");
        var password  = ConsoleUI.PromptPassword("Temporary Password");

        Console.WriteLine("Role              : (1) Admin  (2) Manager  (3) Employee");
        var roleChoice = ConsoleUI.Prompt("Enter choice");
        var roleMap = new[] { "Admin", "Manager", "Employee" };
        if (!int.TryParse(roleChoice, out var ri) || ri < 1 || ri > 3)
        { ConsoleUI.Error("Invalid role."); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.ActionBar("[S] Save", "[B] Back");
        if (ConsoleUI.PromptOption().ToUpper() == "B") return;

        var (data, error) = await api.CreateUserAsync(new CreateUserDto
        {
            FullName          = fullName,
            Email             = email,
            Username          = username,
            TemporaryPassword = password,
            Role              = roleMap[ri - 1]
        });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("Account created. User must change password on first login.");
        ConsoleUI.PressAnyKey();
    }

    private async Task ViewAllUsersAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("ALL USERS");

        var (users, error) = await api.GetUsersAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        var list = users?.ToList() ?? [];

        ConsoleUI.RenderTable(
            ["ID", "Username", "Role", "Status"],
            list.Select(u => new[]
            {
                u.Id.ToString(),
                u.Username,
                u.Role,
                u.IsActive ? "Active" : "Inactive"
            }),
            rightAlignColumnIndexes: [0]);

        ConsoleUI.Divider();
        var active = list.Count(u => u.IsActive);
        Console.WriteLine($"Total: {list.Count}   |   Active: {active}   |   Inactive: {list.Count - active}");
        ConsoleUI.BlankLine();
        ConsoleUI.ActionBar("[R] Reactivate a user", "[B] Back");

        var opt = ConsoleUI.PromptOption();
        if (opt.ToUpper() == "R") await ReactivateUserAsync(list);
    }

    private async Task ReactivateUserAsync(List<UserDto> users)
    {
        ConsoleUI.BlankLine();
        var idStr = ConsoleUI.Prompt("Enter User ID to reactivate");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) { ConsoleUI.Error("User not found."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine($"\nUser: {user.FullName} ({user.Role}) � currently Inactive");
        Console.WriteLine("\nReactivate this account?");
        ConsoleUI.ActionBar("[Y] Yes", "[B] Cancel");
        if (ConsoleUI.PromptOption().ToUpper() != "Y") return;

        var (_, error) = await api.ReactivateUserAsync(id);
        if (error is not null) ConsoleUI.Error(error);
        else
        {
            ConsoleUI.Success($"Account reactivated. {user.FullName} can now log in.");
            ConsoleUI.Info("Note: Previous allocations are NOT restored. Admin must re-allocate manually if needed.");
        }
        ConsoleUI.PressAnyKey();
    }

    private async Task ResetPasswordAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("RESET USER PASSWORD");

        var idStr = ConsoleUI.Prompt("Enter Username or User ID");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        var (users, _) = await api.GetUsersAsync();
        var user = users?.FirstOrDefault(u => u.Id == id);
        if (user is null) { ConsoleUI.Error("User not found."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine($"\nUser found: {user.FullName} ({user.Role})");
        var newPwd = ConsoleUI.PromptPassword("New Temporary Password");

        ConsoleUI.ActionBar("[S] Save", "[B] Back");
        if (ConsoleUI.PromptOption().ToUpper() == "B") return;

        var (_, error) = await api.ResetPasswordAsync(id, new ResetPasswordDto { NewTemporaryPassword = newPwd });
        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("Password reset. User will be prompted to change it on next login.");
        ConsoleUI.PressAnyKey();
    }

    private async Task DeactivateUserAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("DEACTIVATE USER");

        var idStr = ConsoleUI.Prompt("Enter Username or User ID");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        var (users, _) = await api.GetUsersAsync();
        var user = users?.FirstOrDefault(u => u.Id == id);
        if (user is null) { ConsoleUI.Error("User not found."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine($"\nUser found: {user.FullName} ({user.Role})");
        Console.WriteLine($"Status     : {(user.IsActive ? "Active" : "Inactive")}");
        ConsoleUI.BlankLine();
        Console.WriteLine("Are you sure you want to deactivate this account?");
        ConsoleUI.Info("Deactivated users cannot log in. Their data is preserved.");
        ConsoleUI.ActionBar("[Y] Yes, Deactivate", "[B] Back");
        if (ConsoleUI.PromptOption().ToUpper() != "Y") return;

        var (_, error) = await api.DeactivateUserAsync(id);
        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("User deactivated.");
        ConsoleUI.PressAnyKey();
    }
}

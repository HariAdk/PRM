using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Core.DTOs.Auth;

namespace ProjectManagementSystem.Client.Screens;

/// <summary>
/// Forced password change screen � shown before any menu on first login.
/// Cannot be skipped.
/// </summary>
public class ChangePasswordScreen(ApiClient api, SessionContext session)
{
    /// <returns>true if password was changed successfully, false if aborted.</returns>
    public async Task<bool> ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("CHANGE PASSWORD", "You must set a new password to continue.");

            var newPwd     = ConsoleUI.PromptPassword("New Password    ");
            var confirmPwd = ConsoleUI.PromptPassword("Confirm Password");

            ConsoleUI.ActionBar("[S] Save and Continue");
            ConsoleUI.BlankLine();

            var opt = ConsoleUI.PromptOption();
            if (opt.ToUpper() != "S")
            {
                ConsoleUI.Warning("Password change is mandatory. Press S to save.");
                ConsoleUI.PressAnyKey();
                continue;
            }

            var (_, error) = await api.ChangePasswordAsync(session.UserId, new ChangePasswordDto
            {
                NewPassword     = newPwd,
                ConfirmPassword = confirmPwd
            });

            if (error is not null)
            {
                ConsoleUI.Error(error);
                ConsoleUI.PressAnyKey();
                continue;
            }

            session.ClearForceChange();
            ConsoleUI.Success("Password updated. Welcome!");
            ConsoleUI.PressAnyKey();
            return true;
        }
    }
}

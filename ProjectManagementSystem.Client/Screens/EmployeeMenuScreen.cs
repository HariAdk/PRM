using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Client.Screens;

public class EmployeeMenuScreen(ApiClient api, SessionContext session, IScreenFactory screenFactory) : IScreen
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            var now = DateTime.Now.ToString(UiFormats.DisplayDateShort);
            ConsoleUI.DrawBox($"Welcome, {session.FullName}!  |  {now}");

            await ShowReminderAsync();

            ConsoleUI.Divider();
            ConsoleUI.Menu((int)EmployeeMenuAction.SubmitTimesheet, "Submit Timesheet");
            ConsoleUI.Menu((int)EmployeeMenuAction.ViewMyTimesheets, "View My Timesheets");
            ConsoleUI.Menu((int)EmployeeMenuAction.ViewMyAllocations, "View My Allocations");
            ConsoleUI.Menu((int)EmployeeMenuAction.Logout, "Logout");

            if (!MenuOptionParser.TryParse(ConsoleUI.PromptOption(), out EmployeeMenuAction action))
            {
                ConsoleUI.Error("Invalid option.");
                ConsoleUI.PressAnyKey();
                continue;
            }

            if (action == EmployeeMenuAction.Logout)
            {
                session.Clear();
                api.ClearToken();
                return;
            }

            await screenFactory.CreateEmployeeScreen(action).ShowAsync();
        }
    }

    private async Task ShowReminderAsync()
    {
        var (reminder, error) = await api.GetEmployeeReminderAsync();
        if (error is not null || reminder is null) return;

        if (reminder.IsFrozen)
        {
            ConsoleUI.Error(reminder.Message);
            return;
        }

        if (reminder.ShowReminder)
            ConsoleUI.Warning(reminder.Message);
    }
}

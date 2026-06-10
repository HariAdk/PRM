using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Client.Screens.Employee;
using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Client.Screens;

/// <summary>Screen 5 � Employee Menu</summary>
public class EmployeeMenuScreen(ApiClient api, SessionContext session)
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
            ConsoleUI.Menu(1, "Submit Timesheet");
            ConsoleUI.Menu(2, "View My Timesheets");
            ConsoleUI.Menu(3, "View My Allocations");
            ConsoleUI.Menu(4, "Logout");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await new SubmitTimesheetScreen(api).ShowAsync(); break;
                case "2": await new ViewTimesheetsScreen(api).ShowAsync(); break;
                case "3": await new ViewMyAllocationsScreen(api).ShowAsync(); break;
                case "4":
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

    private async Task ShowReminderAsync()
    {
        var (reminder, error) = await api.GetEmployeeReminderAsync();
        if (error is not null || reminder is null || !reminder.ShowReminder) return;
        ConsoleUI.Warning(reminder.Message);
    }
}

using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Client.Screens.Manager;

/// <summary>Screen 4.5 — Restore frozen timesheet submission access</summary>
public class RestoreTimesheetAccessScreen(ApiClient api) : IScreen
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("RESTORE TIMESHEET ACCESS");

            var (frozen, error) = await api.GetManagerFrozenTimesheetsAsync();
            if (error is not null)
            {
                ConsoleUI.Error(error);
                ConsoleUI.PressAnyKey();
                return;
            }

            var list = frozen?.ToList() ?? [];
            if (list.Count == 0)
            {
                Console.WriteLine("No frozen timesheet submissions on your team.");
            }
            else
            {
                ConsoleUI.RenderTable(
                    ["Employee ID", "Employee", "Week Start", "Frozen At"],
                    list.Select(f => new[]
                    {
                        f.EmployeeId.ToString(),
                        f.EmployeeName,
                        f.WeekStartDate.ToString(UiFormats.DisplayDateShort),
                        f.FrozenAt?.ToString(UiFormats.DisplayDateTimeShort) ?? "-"
                    }));
            }

            ConsoleUI.Divider();
            ConsoleUI.Menu(1, "Restore access for an employee");
            ConsoleUI.Menu(2, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1":
                    await RestoreAsync(list);
                    break;
                case "2":
                    return;
                default:
                    ConsoleUI.Error("Invalid option.");
                    ConsoleUI.PressAnyKey();
                    break;
            }
        }
    }

    private async Task RestoreAsync(IReadOnlyList<Core.DTOs.Notification.FrozenTimesheetDto> frozenList)
    {
        ConsoleUI.Divider();
        var employeeIdStr = ConsoleUI.Prompt("Employee ID");
        if (!int.TryParse(employeeIdStr, out var employeeId))
        {
            ConsoleUI.Error("Invalid employee ID.");
            ConsoleUI.PressAnyKey();
            return;
        }

        var match = frozenList.FirstOrDefault(f => f.EmployeeId == employeeId);
        DateTime weekStart;
        if (match is not null)
        {
            weekStart = match.WeekStartDate;
        }
        else
        {
            var weekStr = ConsoleUI.Prompt("Week start (DD-MM-YYYY) or Enter for last completed week");
            if (string.IsNullOrWhiteSpace(weekStr))
            {
                weekStart = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
            }
            else if (!DateOnly.TryParseExact(weekStr, UiFormats.DisplayDate, out var week))
            {
                ConsoleUI.Error("Invalid date format.");
                ConsoleUI.PressAnyKey();
                return;
            }
            else
            {
                weekStart = week.ToDateTime(TimeOnly.MinValue);
            }
        }

        var (_, error) = await api.RestoreTimesheetAccessAsync(new RestoreTimesheetAccessDto
        {
            EmployeeId = employeeId,
            WeekStartDate = weekStart
        });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success(SuccessMessages.TimesheetAccessRestored);
        ConsoleUI.PressAnyKey();
    }
}

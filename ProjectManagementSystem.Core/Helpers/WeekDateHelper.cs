using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Core.Helpers;

public static class WeekDateHelper
{
    public static DateTime GetMondayOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    public static DateTime GetLastCompletedWeekMonday(DateTime referenceDate) =>
        GetMondayOfWeek(referenceDate).AddDays(-7);

    public static DateTime? TryParseWeekStart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!DateTime.TryParse(value, out var parsed))
            throw new FormatException(ErrorMessages.InvalidWeekStartDate);

        return GetMondayOfWeek(parsed);
    }

    public static void EnsureWeekNotInFuture(DateTime weekMonday)
    {
        var currentWeek = GetMondayOfWeek(DateTime.Today);
        if (weekMonday > currentWeek)
            throw new InvalidOperationException(ErrorMessages.FutureWeekTimesheet);
    }
}

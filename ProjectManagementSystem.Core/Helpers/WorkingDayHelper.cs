namespace ProjectManagementSystem.Core.Helpers;

public static class WorkingDayHelper
{
    public static bool IsWorkingDay(DateTime date) =>
        date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;

    public static int CountWorkingDaysSinceWeekEnd(DateOnly weekEndSunday, DateOnly upTo)
    {
        var count = 0;
        var cursor = weekEndSunday.AddDays(1);
        while (cursor <= upTo)
        {
            if (IsWorkingDay(cursor.ToDateTime(TimeOnly.MinValue)))
                count++;
            cursor = cursor.AddDays(1);
        }

        return count;
    }
}

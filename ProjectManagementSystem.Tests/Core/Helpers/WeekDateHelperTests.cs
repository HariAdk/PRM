using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Tests.Core.Helpers;

public class WeekDateHelperTests
{
    [Fact]
    public void GetMondayOfWeek_ReturnsMondayForWednesday()
    {
        var wednesday = new DateTime(2026, 5, 13);
        var monday = WeekDateHelper.GetMondayOfWeek(wednesday);
        Assert.Equal(DayOfWeek.Monday, monday.DayOfWeek);
        Assert.Equal(new DateTime(2026, 5, 11), monday);
    }

    [Fact]
    public void EnsureWeekNotInFuture_ThrowsForFutureWeek()
    {
        var futureMonday = WeekDateHelper.GetMondayOfWeek(DateTime.Today).AddDays(7);
        Assert.Throws<InvalidOperationException>(() => WeekDateHelper.EnsureWeekNotInFuture(futureMonday));
    }

    [Fact]
    public void TryParseWeekStart_ReturnsNullForEmptyInput()
    {
        Assert.Null(WeekDateHelper.TryParseWeekStart(null));
        Assert.Null(WeekDateHelper.TryParseWeekStart("   "));
    }
}

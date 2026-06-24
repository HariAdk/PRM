using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Tests.Application;

public class PasswordExpiryHelperTests
{
    [Fact]
    public void RequiresChange_ReturnsTrueWhenExpiryIsInPast()
    {
        Assert.True(PasswordExpiryHelper.RequiresChange(DateTime.UtcNow.AddMinutes(-1)));
    }

    [Fact]
    public void RequiresChange_ReturnsFalseWhenExpiryIsInFuture()
    {
        Assert.False(PasswordExpiryHelper.RequiresChange(DateTime.UtcNow.AddMonths(3)));
    }
}

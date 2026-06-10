using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Validation;

namespace ProjectManagementSystem.Tests.Core.Validation;

public class ActivityTagValidatorTests
{
    [Fact]
    public void Validate_AcceptsPredefinedTag()
    {
        var ex = Record.Exception(() => ActivityTagValidator.Validate(ActivityTags.All[0]));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ThrowsWhenEmpty(string tags)
    {
        Assert.Throws<BusinessRuleException>(() => ActivityTagValidator.Validate(tags));
    }

    [Fact]
    public void Validate_ThrowsWhenTagNotInList()
    {
        Assert.Throws<ValidationException>(() => ActivityTagValidator.Validate("Not A Real Tag"));
    }

    [Fact]
    public void ValidateEntryTags_SkipsEntriesWithZeroHours()
    {
        var ex = Record.Exception(() =>
            ActivityTagValidator.ValidateEntryTags([(0m, "")]));

        Assert.Null(ex);
    }

    [Fact]
    public void ValidateEntryTags_RequiresTagsWhenHoursPositive()
    {
        Assert.Throws<BusinessRuleException>(() =>
            ActivityTagValidator.ValidateEntryTags([(8m, "")]));
    }
}

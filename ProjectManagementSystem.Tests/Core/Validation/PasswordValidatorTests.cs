using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Validation;

namespace ProjectManagementSystem.Tests.Core.Validation;

public class PasswordValidatorTests
{
    [Fact]
    public void Validate_AcceptsStrongPassword()
    {
        var ex = Record.Exception(() => PasswordValidator.Validate("Admin@1234"));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("alllowercase1")]
    [InlineData("NoDigitsHere")]
    public void Validate_RejectsWeakPassword(string password)
    {
        Assert.Throws<BusinessRuleException>(() => PasswordValidator.Validate(password));
    }
}

using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;

namespace ProjectManagementSystem.Core.Validation;

public static class PasswordValidator
{
    public static void Validate(string password)
    {
        if (password.Length < PasswordPolicy.MinLength)
            throw new BusinessRuleException(ErrorMessages.PasswordTooShort());

        if (!password.Any(char.IsUpper))
            throw new BusinessRuleException(ErrorMessages.PasswordMissingUppercase);

        if (!password.Any(char.IsDigit))
            throw new BusinessRuleException(ErrorMessages.PasswordMissingDigit);
    }
}

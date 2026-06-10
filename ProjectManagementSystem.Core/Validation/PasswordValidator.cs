using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Core.Validation;

public static class PasswordValidator
{
    public static void Validate(string password)
    {
        if (password.Length < PasswordPolicy.MinLength)
            throw new InvalidOperationException(ErrorMessages.PasswordTooShort());

        if (!password.Any(char.IsUpper))
            throw new InvalidOperationException(ErrorMessages.PasswordMissingUppercase);

        if (!password.Any(char.IsDigit))
            throw new InvalidOperationException(ErrorMessages.PasswordMissingDigit);
    }
}

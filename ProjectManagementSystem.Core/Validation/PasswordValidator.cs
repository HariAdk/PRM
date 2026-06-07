using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Core.Validation;

public static class PasswordValidator
{
    public static void Validate(string password)
    {
        if (password.Length < PasswordPolicy.MinLength)
            throw new InvalidOperationException(
                $"Password must be at least {PasswordPolicy.MinLength} characters.");

        if (!password.Any(char.IsUpper))
            throw new InvalidOperationException("Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsDigit))
            throw new InvalidOperationException("Password must contain at least one number.");
    }
}

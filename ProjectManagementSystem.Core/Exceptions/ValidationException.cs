namespace ProjectManagementSystem.Core.Exceptions;

public sealed class ValidationException : AppException
{
    public ValidationException(string userMessage)
        : base(userMessage, AppErrorKind.BadRequest)
    {
    }
}

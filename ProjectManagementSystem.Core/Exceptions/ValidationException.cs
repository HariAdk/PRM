namespace ProjectManagementSystem.Core.Exceptions;

/// <summary>Invalid input format or value supplied by the client.</summary>
public sealed class ValidationException : AppException
{
    public ValidationException(string userMessage)
        : base(userMessage, AppErrorKind.BadRequest)
    {
    }
}

namespace ProjectManagementSystem.Core.Exceptions;

public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string userMessage)
        : base(userMessage, AppErrorKind.Forbidden)
    {
    }
}

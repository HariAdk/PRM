namespace ProjectManagementSystem.Core.Exceptions;

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string userMessage)
        : base(userMessage, AppErrorKind.Unauthorized)
    {
    }
}

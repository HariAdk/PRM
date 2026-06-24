namespace ProjectManagementSystem.Core.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string userMessage)
        : base(userMessage, AppErrorKind.NotFound)
    {
    }
}

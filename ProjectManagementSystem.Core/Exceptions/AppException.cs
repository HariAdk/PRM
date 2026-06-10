namespace ProjectManagementSystem.Core.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string userMessage, AppErrorKind kind)
        : base(userMessage)
    {
        Kind = kind;
    }

    public AppErrorKind Kind { get; }
}

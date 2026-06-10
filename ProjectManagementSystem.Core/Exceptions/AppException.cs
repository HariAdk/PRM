namespace ProjectManagementSystem.Core.Exceptions;

/// <summary>
/// Base for domain errors with a user-friendly <see cref="Exception.Message"/>.
/// Services and repositories throw these; the API maps them to HTTP responses.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string userMessage, AppErrorKind kind)
        : base(userMessage)
    {
        Kind = kind;
    }

    public AppErrorKind Kind { get; }
}

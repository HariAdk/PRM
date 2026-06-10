namespace ProjectManagementSystem.Core.Exceptions;

/// <summary>HTTP-agnostic error category — mapped to status codes in the API layer.</summary>
public enum AppErrorKind
{
    NotFound,
    BadRequest,
    Unauthorized,
    Forbidden,
    Internal
}

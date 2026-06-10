using System.Net;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;

namespace ProjectManagementSystem.Middleware;

/// <summary>
/// Maps exceptions to HTTP status codes and user-facing messages.
/// Kept separate from <see cref="ExceptionHandlingMiddleware"/> so mapping rules are easy to test and extend.
/// </summary>
internal static class ExceptionResponseMapper
{
    public static ExceptionMappingResult Map(Exception exception) =>
        exception switch
        {
            AppException app => MapAppException(app),
            KeyNotFoundException ex => new(HttpStatusCode.NotFound, ex.Message, LogLevel.Warning),
            UnauthorizedAccessException ex => MapLegacyUnauthorized(ex.Message),
            InvalidOperationException ex => new(HttpStatusCode.BadRequest, ex.Message, LogLevel.Warning),
            FormatException ex => new(HttpStatusCode.BadRequest, ex.Message, LogLevel.Warning),
            ArgumentException ex => new(HttpStatusCode.BadRequest, ex.Message, LogLevel.Warning),
            _ => new(HttpStatusCode.InternalServerError, ErrorMessages.UnexpectedError, LogLevel.Error)
        };

    private static ExceptionMappingResult MapAppException(AppException app) =>
        new(app.Kind.ToHttpStatusCode(), app.Message, LogLevel.Warning);

    private static ExceptionMappingResult MapLegacyUnauthorized(string message) =>
        message is ErrorMessages.CannotChangeOtherUserPassword or ErrorMessages.EmployeeNotOnTeam
            ? new(HttpStatusCode.Forbidden, message, LogLevel.Warning)
            : new(HttpStatusCode.Unauthorized, message, LogLevel.Warning);

    private static HttpStatusCode ToHttpStatusCode(this AppErrorKind kind) => kind switch
    {
        AppErrorKind.NotFound => HttpStatusCode.NotFound,
        AppErrorKind.BadRequest => HttpStatusCode.BadRequest,
        AppErrorKind.Unauthorized => HttpStatusCode.Unauthorized,
        AppErrorKind.Forbidden => HttpStatusCode.Forbidden,
        AppErrorKind.Internal => HttpStatusCode.InternalServerError,
        _ => HttpStatusCode.InternalServerError
    };
}

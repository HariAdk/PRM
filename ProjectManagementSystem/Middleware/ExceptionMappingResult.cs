using System.Net;

namespace ProjectManagementSystem.Middleware;

internal sealed record ExceptionMappingResult(
    HttpStatusCode StatusCode,
    string UserMessage,
    LogLevel LogLevel);

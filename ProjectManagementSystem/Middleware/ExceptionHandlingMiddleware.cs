using System.Net;
using System.Text.Json;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;

namespace ProjectManagementSystem.Middleware;

/// <summary>
/// Maps unhandled exceptions to consistent <see cref="ApiResponse{T}"/> JSON and HTTP status codes.
/// Controllers can throw standard BCL exceptions; this middleware handles the HTTP boundary.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, logLevel) = MapException(exception);

        if (logLevel == LogLevel.Error)
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning("Request failed: {Message} ({Method} {Path})", message, context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var body = ApiResponse<object>.Fail(message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOpts));
    }

    private static (HttpStatusCode StatusCode, string Message, LogLevel LogLevel) MapException(Exception exception) =>
        exception switch
        {
            KeyNotFoundException ex => (HttpStatusCode.NotFound, ex.Message, LogLevel.Warning),
            InvalidOperationException ex => (HttpStatusCode.BadRequest, ex.Message, LogLevel.Warning),
            FormatException ex => (HttpStatusCode.BadRequest, ex.Message, LogLevel.Warning),
            ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message, LogLevel.Warning),
            UnauthorizedAccessException ex => (MapUnauthorized(ex.Message), ex.Message, LogLevel.Warning),
            _ => (HttpStatusCode.InternalServerError, ErrorMessages.UnexpectedError, LogLevel.Error)
        };

    private static HttpStatusCode MapUnauthorized(string message) =>
        message is ErrorMessages.CannotChangeOtherUserPassword or ErrorMessages.EmployeeNotOnTeam
            ? HttpStatusCode.Forbidden
            : HttpStatusCode.Unauthorized;
}

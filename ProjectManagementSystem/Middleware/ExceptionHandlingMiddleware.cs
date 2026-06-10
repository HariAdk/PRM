using System.Text.Json;
using ProjectManagementSystem.Core.DTOs.Common;

namespace ProjectManagementSystem.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns consistent <see cref="ApiResponse{T}"/> JSON.
/// Mapping rules live in <see cref="ExceptionResponseMapper"/>.
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
        var mapping = ExceptionResponseMapper.Map(exception);

        if (mapping.LogLevel == LogLevel.Error)
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning("Request failed: {Message} ({Method} {Path})", mapping.UserMessage, context.Request.Method, context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)mapping.StatusCode;

        var body = ApiResponse<object>.Fail(mapping.UserMessage);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOpts));
    }
}

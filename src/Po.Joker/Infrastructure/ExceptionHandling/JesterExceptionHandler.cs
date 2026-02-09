using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Po.Joker.Infrastructure.ExceptionHandling;

/// <summary>
/// Global exception handler that converts all unhandled exceptions to RFC 7807 Problem Details
/// with themed medieval messages for user-facing errors.
/// </summary>
public sealed class JesterExceptionHandler : IExceptionHandler
{
    private readonly ILogger<JesterExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public JesterExceptionHandler(ILogger<JesterExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
            traceId,
            httpContext.Request.Path);

        var (statusCode, title, detail) = MapExceptionToResponse(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _environment.IsDevelopment() ? exception.Message : detail,
            Instance = httpContext.Request.Path,
            Type = $"https://pojoker.app/errors/{exception.GetType().Name.ToLowerInvariant()}"
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["jesterCode"] = exception.GetType().Name;

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            SpeechlessJesterException speechless => (451, speechless.Message, GetSpeechlessDetail(speechless)),
            ArgumentNullException => (400, "The Jester Received Empty Scrolls!", "A required value was missing from your request."),
            ArgumentException => (400, "The Jester Cannot Read This Script!", "The provided value was invalid."),
            InvalidOperationException => (400, "The Court Forbids This Action!", "This operation is not permitted in the current state."),
            HttpRequestException => (503, "The Royal Scroll is Missing!", "Unable to reach the external joke source. The courier may be lost."),
            TaskCanceledException => (408, "The Jester Grew Weary Waiting!", "The request took too long and was cancelled."),
            TimeoutException => (408, "The Hourglass Has Emptied!", "The operation timed out."),
            UnauthorizedAccessException => (401, "Halt! Who Goes There?", "You are not authorized to access this resource."),
            KeyNotFoundException => (404, "The Scroll Was Not Found!", "The requested resource does not exist."),
            NotImplementedException => (501, "The Jester Has Not Learned This Trick!", "This feature is not yet implemented."),
            _ => (500, "The Jester Has Tripped!", "An unexpected error occurred. The court apologizes for the mishap.")
        };
    }

    private static string GetSpeechlessDetail(SpeechlessJesterException speechless)
    {
        var detail = "The Court's content policy has silenced this jest. The Jester shall try again with more courtly humor.";
        if (!string.IsNullOrEmpty(speechless.FilterCategory))
        {
            detail += $" (Category: {speechless.FilterCategory})";
        }
        return detail;
    }
}

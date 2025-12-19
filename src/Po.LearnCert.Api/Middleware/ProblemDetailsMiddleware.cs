using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Po.LearnCert.Api.Middleware;

/// <summary>
/// Global exception handling middleware that returns RFC 7807 Problem Details responses.
/// </summary>
public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ProblemDetailsMiddleware(
        RequestDelegate next,
        ILogger<ProblemDetailsMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(context, exception);
        var statusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        if (statusCode >= (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception mapped to {StatusCode}", statusCode);
        }
        else if (statusCode == (int)HttpStatusCode.NotFound)
        {
            _logger.LogInformation(
                "Handled {ExceptionType} mapped to {StatusCode}: {Message}",
                exception.GetType().Name,
                statusCode,
                exception.Message);
        }
        else
        {
            _logger.LogWarning(
                "Handled {ExceptionType} mapped to {StatusCode}: {Message}",
                exception.GetType().Name,
                statusCode,
                exception.Message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid Operation", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", "You are not authorized to access this resource"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found", exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request", exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error",
                  _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred")
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        // Add exception details in development mode
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        // Add trace identifier for correlation
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        return problemDetails;
    }
}

/// <summary>
/// Extension methods for registering Problem Details middleware.
/// </summary>
public static class ProblemDetailsMiddlewareExtensions
{
    public static IApplicationBuilder UseProblemDetailsExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ProblemDetailsMiddleware>();
    }
}

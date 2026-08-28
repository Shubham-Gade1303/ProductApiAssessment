using System.Net;
using System.Text.Json;
using FluentValidation;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred.");

            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            ValidationException =>
                StatusCodes.Status400BadRequest,

            KeyNotFoundException =>
                StatusCodes.Status404NotFound,

            ArgumentException =>
                StatusCodes.Status400BadRequest,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        var response = new
        {
            statusCode,
            message = GetMessage(
                exception,
                statusCode),
            timestamp = DateTime.UtcNow
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }

    private static string GetMessage(
        Exception exception,
        int statusCode)
    {
        if (statusCode ==
            StatusCodes.Status500InternalServerError)
        {
            return "An unexpected error occurred.";
        }

        if (exception is ValidationException validationException)
        {
            return string.Join(
                " ",
                validationException.Errors
                    .Select(error => error.ErrorMessage));
        }

        return exception.Message;
    }
}
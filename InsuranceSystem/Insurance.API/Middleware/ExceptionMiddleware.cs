using System.Net;
using System.Text.Json;

namespace Insurance.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = exception.Message;

        if (message.Contains("exists") || message.Contains("must be") || message.Contains("Only"))
            statusCode = HttpStatusCode.BadRequest;
        else if (message.Contains("credentials") || message.Contains("not found") || message.Contains("Unauthorized"))
            statusCode = HttpStatusCode.Unauthorized;

        var response = new
        {
            success = false,
            message = message,
            statusCode = (int)statusCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
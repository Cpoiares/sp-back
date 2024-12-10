using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using sp_back.models.Exceptions;
using sp_back.models.Models.Error;

namespace sp_back_api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case NotFoundException notFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                errorResponse.Message = notFoundException.Message;
                break;

            case ValidationException validationException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Message = validationException.Message;
                break;
            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Message = exception.Message;
                break;
        }

        await response.WriteAsJsonAsync(errorResponse);
    }
}
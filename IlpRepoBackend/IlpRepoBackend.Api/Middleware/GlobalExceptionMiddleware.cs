using IlpRepoBackend.Application.CustomeException;
using System.Net;
using System.Text.Json;

namespace IlpRepoBackend.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string message;
            string? stackTrace = null;

            switch (exception)
            {
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = notFoundException.Message;
                    break;
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = JsonSerializer.Serialize(validationException.Errors);
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = exception.Message;
                    stackTrace = exception.StackTrace;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                StatusCode = (int)statusCode,
                Message = message,
                StackTrace = stackTrace,
                InnerException = exception.InnerException?.Message
            });

            return context.Response.WriteAsync(result);
        }
    }
}

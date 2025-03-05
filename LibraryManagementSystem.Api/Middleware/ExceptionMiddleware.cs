using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");

                // Default to 500 unless we handle it below
                var statusCode = HttpStatusCode.InternalServerError;
                var message = "An unexpected error occurred. Please try again later.";

                // Example: handle certain exceptions more specifically
                if (ex is KeyNotFoundException)
                {
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message; // or a generic "Resource not found" message
                }
                else if (ex is ArgumentException)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                }
                else if (ex is InvalidOperationException)
                {
                    statusCode = HttpStatusCode.Conflict;
                    message = ex.Message;
                }

                // Prepare response
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                var response = new
                {
                    statusCode = context.Response.StatusCode,
                    message = message
                    // For production, you might omit ex.Message to avoid leaking sensitive details.
                };

                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }
}
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LibraryManagementSystem.Application.Common;

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

                // Default to 500 Internal Server Error
                var statusCode = HttpStatusCode.InternalServerError;
                var message = "An unexpected error occurred. Please try again later.";

                // Handle specific exceptions
                if (ex is KeyNotFoundException)
                {
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message; // Or use a generic message like "Resource not found"
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

                // Prepare standardized API response
                var response = ApiResponse<object>.ErrorResponse(message, new List<string> { $"Status Code: {(int)statusCode}" });

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }
}
using Exam.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Exam.Infrastructure.Middleware
{
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

            // ==============================
            // Custom Not Found Exception
            // ==============================
            catch (ItemNotFoundException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    ex.Message
                );
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    ex.Message
                );
            }

            // ==============================
            // Bad Request Errors
            // ==============================

            // ==============================
            // Security Errors
            // ==============================
            catch (UnauthorizedAccessException)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    "Access denied"
                );
            }

            // ==============================
            // Database Update Errors
            // ==============================
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error occurred.");
                await HandleDbExceptionAsync(context, ex);
            }


            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later."
                );
            }
        }

        private async Task HandleDbExceptionAsync(HttpContext context, DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 2627: // Unique constraint
                        await HandleExceptionAsync(context,
                            StatusCodes.Status409Conflict,
                            "Duplicate value");
                        break;

                    case 515: // Cannot insert null
                        await HandleExceptionAsync(context,
                            StatusCodes.Status400BadRequest,
                            "Required field is missing");
                        break;

                    case 547: // Foreign key violation
                        await HandleExceptionAsync(context,
                            StatusCodes.Status400BadRequest,
                            "Invalid reference data");
                        break;

                    default:
                        await HandleExceptionAsync(context,
                            StatusCodes.Status500InternalServerError,
                            "Database error occurred");
                        break;
                }
            }
            else
            {
                await HandleExceptionAsync(context,
                    StatusCodes.Status500InternalServerError,
                    "Database error occurred");
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            int statusCode,
            string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = Exam.Application.Dto.Common.ApiResponse.FailureResponse(message, statusCode);

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
        }
    }
}
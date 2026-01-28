using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Exam.Infrastructure.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DbUpdateException ex)
            {
                await HandleDbExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await WriteError(context, $"Unexpected error occurred: {ex.Message}");
            }
        }

        private async Task HandleDbExceptionAsync(HttpContext context, DbUpdateException ex)
        {
            context.Response.ContentType = "application/json";

            if (ex.InnerException is SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 2627: // Unique constraint
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        await WriteError(context, "Duplicate value");
                        break;

                    case 515: // Cannot insert null
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await WriteError(context, "Required field is missing");
                        break;

                    case 547: // Foreign key violation
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await WriteError(context, "Invalid reference data");
                        break;

                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await WriteError(context, "Database error occurred");
                        break;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await WriteError(context, "Database error occurred");
            }
        }

        private static Task WriteError(HttpContext context, string message)
        {
            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                error = message
            }));
        }
    }
}

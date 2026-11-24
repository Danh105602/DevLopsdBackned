using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Exceptions;

namespace WebApplication1.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
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
                if (context.Response.HasStarted)
                {
                    throw;
                }

                var (statusCode, message) = Map(ex);

                // Lỗi nghiệp vụ (4xx) ghi Warning, lỗi hệ thống ghi Error
                if (statusCode >= 500)
                {
                    _logger.LogError(
                        ex, "Unhandled Exception: {Message}", ex.Message);
                }
                else
                {
                    _logger.LogWarning(
                        "Business error {StatusCode}: {Message}",
                        statusCode, message);
                }

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    success = false,
                    statusCode,
                    message
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response));
            }
        }

        private static (int StatusCode, string Message) Map(Exception ex)
        {
            // 1. Exception nghiệp vụ tự định nghĩa
            if (ex is AppException app)
            {
                return (app.StatusCode, app.Message);
            }

            // 2. Lỗi ràng buộc SQL Server
            if (ex is DbUpdateException dbEx &&
                dbEx.InnerException is SqlException sql)
            {
                // 547: vi phạm khóa ngoại (tham chiếu sai hoặc đang được sử dụng)
                if (sql.Number == 547)
                {
                    return (409,
                        "Dữ liệu tham chiếu không tồn tại hoặc đang được sử dụng.");
                }

                // 2601, 2627: trùng giá trị duy nhất
                if (sql.Number is 2601 or 2627)
                {
                    return (409, "Dữ liệu đã tồn tại.");
                }
            }

            // 3. Còn lại: lỗi hệ thống
            return (500, "Đã xảy ra lỗi hệ thống.");
        }
    }
}
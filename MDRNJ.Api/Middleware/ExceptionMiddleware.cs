using MDRNJ.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDRNJ.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (EmailNotVerifiedException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (ArgumentException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex) when (IsDatabaseUnavailable(ex))
            {
                // Usually Azure SQL resuming after an auto-pause; the app retries 503s
                _logger.LogWarning(ex, "Database unavailable");
                await WriteErrorAsync(context, HttpStatusCode.ServiceUnavailable,
                    "The server is waking up. Try again in a moment.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                var message = _env.IsDevelopment()
                    ? $"{ex.GetType().Name}: {ex.Message} | Inner: {ex.InnerException?.Message}"
                    : "An unexpected error occurred.";
                await WriteErrorAsync(context, HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>SQL errors meaning "can't reach the database right now", not a bad query.</summary>
        private static readonly HashSet<int> UnavailableSqlErrors = new()
        {
            -2,                         // timeout
            53, 233, 10053, 10054, 10060, // network / connection dropped
            4060, 40613, 40197, 40501,  // database unavailable / resuming / busy
            49918, 49919, 49920         // service busy
        };

        private static bool IsDatabaseUnavailable(Exception ex)
        {
            for (var e = ex; e is not null; e = e.InnerException)
                if (e is RetryLimitExceededException
                    || (e is SqlException sql && UnavailableSqlErrors.Contains(sql.Number)))
                    return true;
            return false;
        }

        private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
        {
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            var body = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(body);
        }
    }
}

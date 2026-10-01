using MDRNJ.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                var message = _env.IsDevelopment()
                    ? $"{ex.GetType().Name}: {ex.Message} | Inner: {ex.InnerException?.Message}"
                    : "An unexpected error occurred.";
                await WriteErrorAsync(context, HttpStatusCode.InternalServerError, message);
            }
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

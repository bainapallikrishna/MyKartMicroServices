using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SharedLibrary.Common;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            // Try to obtain correlation id from context (set by CorrelationIdMiddleware) or request header
            var correlationId = context.Items.ContainsKey(LoggingConstants.CorrelationIdProperty)
                ? context.Items[LoggingConstants.CorrelationIdProperty]?.ToString()
                : (context.Request.Headers.TryGetValue(LoggingConstants.CorrelationIdHeader, out var headerVal) ? headerVal.ToString() : null);

            _logger.LogError(ex, "{Message} - CorrelationId: {CorrelationId}", LoggingConstants.UnhandledException, correlationId);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var payload = JsonSerializer.Serialize(new { message = "Internal server error", correlationId });
                await context.Response.WriteAsync(payload);
            }

            return;
        }
    }
}


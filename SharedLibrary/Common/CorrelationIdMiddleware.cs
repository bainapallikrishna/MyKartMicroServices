using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Middleware to generate and propagate correlation IDs for request tracing.
    /// </summary>
    public sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Request.Headers.TryGetValue(
                LoggingConstants.CorrelationIdHeader, 
                out var headerValue) 
                ? headerValue.ToString() 
                : GenerateCorrelationId();

            context.Items[LoggingConstants.CorrelationIdProperty] = correlationId;
            context.Response.Headers.Add(LoggingConstants.CorrelationIdHeader, correlationId);

            await _next(context);
        }

        private static string GenerateCorrelationId()
        {
            return $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
    }
}

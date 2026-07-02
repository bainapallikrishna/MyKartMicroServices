using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Enhanced request logging middleware with correlation IDs, request/response bodies, and performance metrics.
    /// </summary>
    public sealed class EnhancedRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EnhancedRequestLoggingMiddleware> _logger;
        private readonly LoggingConfiguration _config;

        public EnhancedRequestLoggingMiddleware(RequestDelegate next, ILogger<EnhancedRequestLoggingMiddleware> logger, LoggingConfiguration config = null)
        {
            _next = next;
            _logger = logger;
            _config = config ?? new LoggingConfiguration();
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Items[LoggingConstants.CorrelationIdProperty]?.ToString() ?? "unknown";
            var sw = Stopwatch.StartNew();

            try
            {
                // Log request details
                var requestHeaders = context.Request.Headers
                    .ToDictionary(h => h.Key, h => h.Value.ToString());

                if (_config.LogRequestHeaders)
                {
                    requestHeaders = SensitiveDataMasker.MaskHeaders(requestHeaders);
                }

                _logger.LogInformation(
                    LoggingConstants.RequestStarted,
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    correlationId);

                // Log request body if configured and applicable
                if (_config.LogRequestBody && IsLoggableContentType(context.Request.ContentType))
                {
                    await LogRequestBody(context, correlationId);
                }

                // Process request
                await _next(context);

                sw.Stop();

                // Log response
                var statusCode = context.Response.StatusCode;
                var logLevel = statusCode >= 500 ? LogLevel.Error : 
                               statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

                _logger.Log(logLevel,
                    LoggingConstants.RequestCompleted,
                    context.Request.Method,
                    context.Request.Path.Value,
                    statusCode,
                    sw.ElapsedMilliseconds,
                    correlationId);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    LoggingConstants.RequestFailed,
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response?.StatusCode ?? 500,
                    sw.ElapsedMilliseconds,
                    correlationId);
                throw;
            }
        }

        private async Task LogRequestBody(HttpContext context, string correlationId)
        {
            try
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(body))
                {
                    var maskedBody = SensitiveDataMasker.MaskSensitiveFieldsInJson(body);
                    _logger.LogDebug("Request body: {RequestBody} - CorrelationId: {CorrelationId}", maskedBody, correlationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log request body - CorrelationId: {CorrelationId}", correlationId);
            }
        }

        private static bool IsLoggableContentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            var loggableTypes = new[] { "application/json", "application/x-www-form-urlencoded", "text/xml", "application/xml" };
            return loggableTypes.Any(type => contentType.Contains(type));
        }
    }

    /// <summary>
    /// Configuration for enhanced request logging.
    /// </summary>
    public class LoggingConfiguration
    {
        public bool LogRequestBody { get; set; } = true;
        public bool LogRequestHeaders { get; set; } = true;
        public bool LogResponseBody { get; set; } = false;
        public int MaxBodyLogSize { get; set; } = 4096; // 4KB
    }
}

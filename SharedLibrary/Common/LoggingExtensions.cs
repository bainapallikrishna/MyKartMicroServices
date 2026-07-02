using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Extension methods for registering and configuring structured logging.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configures structured logging with file and console sinks.
        /// </summary>
        public static IServiceCollection AddStructuredLogging(
            this IServiceCollection services,
            IConfiguration configuration,
            string applicationName)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Get logging configuration section or use defaults
            var loggingConfig = new LoggingConfiguration();
            var loggingSection = configuration.GetSection("Logging");
            loggingSection?.Bind(loggingConfig);

            services.AddSingleton(loggingConfig);

            // Configure ILogger
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);

                // Add structured console formatting
                builder.AddConsoleFormatter<JsonConsoleFormatter, JsonConsoleFormatterOptions>();
                builder.AddConsole(options =>
                {
                    options.FormatterName = nameof(JsonConsoleFormatter);
                });
            });

            return services;
        }

        /// <summary>
        /// Registers correlation ID and enhanced logging middleware.
        /// </summary>
        public static IApplicationBuilder UseStructuredLogging(
            this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            // Add correlation ID middleware first (generates IDs)
            app.UseMiddleware<CorrelationIdMiddleware>();

            // Add enhanced request logging
            var loggingConfig = app.ApplicationServices.GetService<LoggingConfiguration>();
            app.UseMiddleware<EnhancedRequestLoggingMiddleware>(loggingConfig);

            return app;
        }

        /// <summary>
        /// Gets the correlation ID from the current HTTP context.
        /// </summary>
        public static string GetCorrelationId(this IHttpContextAccessor contextAccessor)
        {
            return contextAccessor?.HttpContext?.Items[LoggingConstants.CorrelationIdProperty]?.ToString() 
                ?? "unknown";
        }
    }
}

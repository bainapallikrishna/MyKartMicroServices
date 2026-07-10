using Microsoft.AspNetCore.Builder;

namespace SharedLibrary.Common;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }

    public static IApplicationBuilder UseSharedLogging(this IApplicationBuilder app)
    {
        // Correlation id should be added before any logging/exception handling so it is available in logs
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}


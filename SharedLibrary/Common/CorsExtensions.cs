using Microsoft.Extensions.DependencyInjection;

namespace SharedLibrary.Common;

public static class CorsExtensions
{
    public const string AngularPolicy = "AngularApp";

    public static IServiceCollection AddMyKartCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(AngularPolicy, policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:4200",
                        "http://127.0.0.1:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using UserMicroservices.Models;
using UserMicroservices.Repository;

using SharedLibrary.Common;
using Microsoft.Extensions.Logging;

namespace UserMicroservices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var env = builder.Environment.EnvironmentName;
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Configure structured logging
            builder.Services.AddStructuredLogging(builder.Configuration, "UserMicroservice");

            builder.Services.AddControllers();
            // JWT Authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);
            // Remove gRPC server registration; this service will expose HTTP REST endpoints instead
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "User Service", Version = "v1" });
                // Add JWT Authorization to Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new string[] {}
                    }
                });
            });

            // Register context accessor and propagation handler outside of the SwaggerGen options
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthorizationPropagationHandler>();
            builder.Services.AddHttpClient("PropagatingClient").AddHttpMessageHandler<AuthorizationPropagationHandler>();
            builder.Services.AddDbContext<UserDBContext>(options =>
               options.UseSqlServer(
                   builder.Configuration.GetConnectionString("UserDBConnectionString"),
                   sqlOptions => sqlOptions.EnableRetryOnFailure(
                       maxRetryCount: 5,
                       maxRetryDelay: TimeSpan.FromSeconds(30),
                       errorNumbersToAdd: null
                   )
               ));
            builder.Services.AddScoped<UserRepository>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });
            builder.Services.AddRedisCache(builder.Configuration);
            // Register ICacheService implementation for distributed caching
            builder.Services.AddSingleton<SharedLibrary.Common.ICacheService, SharedLibrary.Common.CacheService>();
            var app = builder.Build();

            // Configure structured logging middleware
            app.UseStructuredLogging();

            // Logging
            app.UseSharedLogging();

            // ✅ Auto migrate on startup — same as CategoryService
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<UserDBContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            app.UseGlobalExceptionHandling();
            app.UseRequestLogging();
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthorization();
            app.MapControllers();
            // No gRPC services mapped - user service exposes REST controllers
            app.Run();
        }
    }
}
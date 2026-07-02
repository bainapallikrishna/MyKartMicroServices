using Microsoft.EntityFrameworkCore;

using PurchaseMicroservices.Models;
using PurchaseMicroservices.Repository;
using SharedLibrary.Common;

namespace PurchaseMicroservices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // ✅ Environment-aware config loading
            var env = builder.Environment.EnvironmentName;
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Configure structured logging
            builder.Services.AddStructuredLogging(builder.Configuration, "PurchaseMicroservice");

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthorizationPropagationHandler>();
            // JWT Authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Purchase Service", Version = "v1" });
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
            builder.Services.AddDbContext<PurchaseDBContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("PurchaseDBConnectionString"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    )
                ));

            builder.Services.AddScoped<PurchaseRepository>();
            builder.Services.AddHttpClient();
            // Named HttpClient that propagates Authorization header
            builder.Services.AddHttpClient("PropagatingClient").AddHttpMessageHandler<AuthorizationPropagationHandler>();

            // Redis Cache Configuration
            builder.Services.AddRedisCache(builder.Configuration);
            // Register ICacheService implementation for distributed caching
            builder.Services.AddSingleton<SharedLibrary.Common.ICacheService, SharedLibrary.Common.CacheService>();

            // gRPC client removed; calling Product service via HTTP REST using named HttpClient 'PropagatingClient'

            var app = builder.Build();

            // Configure structured logging middleware
            app.UseStructuredLogging();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<PurchaseDBContext>();
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
            // Shared logging middleware from SharedLibrary.Common
            app.UseSharedLogging();
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
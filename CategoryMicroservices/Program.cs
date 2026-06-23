using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using CategoryMicroservices.Models;
using CategoryMicroservices.Repository;
using CategoryMicroservices.Grpc;
using SharedLibrary.Common;
namespace CategoryMicroservices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging to use console and ensure consistent logging across microservices
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // ✅ Environment-aware config loading
            var env = builder.Environment.EnvironmentName;
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.Services.AddControllers();
            // JWT Authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();

            // Register context accessor and propagation handler before building the container
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthorizationPropagationHandler>();
            builder.Services.AddHttpClient("PropagatingClient").AddHttpMessageHandler<AuthorizationPropagationHandler>();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Category Service", Version = "v1" });
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

            builder.Services.AddDbContext<CategoryDBContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("CategoryDBConnectionString"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)
                ));

            builder.Services.AddScoped<CategoryRepository>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });
            builder.Services.AddRedisCache(builder.Configuration);
            var app = builder.Build();

            // Use centralized shared logging middleware
            app.UseSharedLogging();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<CategoryDBContext>();
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
            app.MapGrpcService<CategoryGrpcService>();
            app.Run();
        }
    }
}
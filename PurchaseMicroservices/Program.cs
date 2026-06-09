using Microsoft.EntityFrameworkCore;
using MyKart.Contracts.Product;
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

            // Configure logging to use console and clear default providers
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

      
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // ✅ Environment-aware config loading
            var env = builder.Environment.EnvironmentName;
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

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

            builder.Services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options =>
            {
                // ✅ Now reads correctly from Development or Docker config
                var address = builder.Configuration.GetValue<string>("Grpc:ProductService");
                options.Address = new Uri(address);
            });

            var app = builder.Build();

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
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

            // ✅ Environment-aware config loading
            var env = builder.Environment.EnvironmentName;
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Purchase Service", Version = "v1" });
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
            app.UseSwagger();
            //app.UseSwaggerUI();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
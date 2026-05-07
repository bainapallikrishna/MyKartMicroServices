using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using UserMicroservices.Models;
using UserMicroservices.Repository;
using UserMicroservices.Grpc;
using SharedLibrary.Common;

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
            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "User Service", Version = "v1" });
            });
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

            var app = builder.Build();

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
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthorization();
            app.MapControllers();
            app.MapGrpcService<UserGrpcService>();
            app.Run();
        }
    }
}
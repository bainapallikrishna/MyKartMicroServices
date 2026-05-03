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

            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Category Service", Version = "v1" });
            });

            builder.Services.AddDbContext<CategoryDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("CategoryDBConnectionString")));

            builder.Services.AddScoped<CategoryRepository>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<CategoryDBContext>();
                    context.Database.Migrate();  // creates DB + runs all migrations automatically
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
            app.MapGrpcService<CategoryGrpcService>();
            app.Run();
        }
    }
}
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

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();

            // Configure Swagger before Build
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

            // Configure middleware
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseGlobalExceptionHandling();
            app.UseRequestLogging();
            app.MapControllers();
            app.MapGrpcService<CategoryGrpcService>();
            app.Run();
        }
    }
}
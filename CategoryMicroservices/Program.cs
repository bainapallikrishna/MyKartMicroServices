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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // ✅ Register DbContext properly
            builder.Services.AddDbContext<CategoryDBContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("CategoryDBConnectionString")
                ));

            // ✅ Register repository correctly
            builder.Services.AddScoped<CategoryRepository>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
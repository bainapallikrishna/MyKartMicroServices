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

            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();
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

            // Fixed title — was showing "Purchase Service" for User
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "User Service", Version = "v1" });
            });

            var app = builder.Build();

            // Always enable swagger
            app.UseSwagger();
            app.UseSwaggerUI();
            // REMOVED UseHttpsRedirection
            app.UseAuthorization();
            app.MapControllers();
            app.MapGrpcService<UserGrpcService>();
            app.UseGlobalExceptionHandling();
            app.UseRequestLogging();
            app.Run();
        }
    }
}
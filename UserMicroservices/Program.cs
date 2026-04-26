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

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<UserDBContext>(options =>
               options.UseSqlServer(
                   builder.Configuration.GetConnectionString("UserDBConnectionString"),
                   sqlOptions =>
                   {
                       sqlOptions.EnableRetryOnFailure(
                           maxRetryCount: 5,
                           maxRetryDelay: TimeSpan.FromSeconds(30),
                           errorNumbersToAdd: null
                       );
                   }
               ));
            // Register repository as scoped (not singleton)
            builder.Services.AddScoped<UserRepository>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<UserGrpcService>();

            app.UseGlobalExceptionHandling();
            app.UseRequestLogging();

            app.Run();
        }
    }
}

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

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
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
                var address = builder.Configuration.GetValue<string>("Grpc:ProductService");
                options.Address = new Uri(address);
            });

            // ADD before Build()
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Purchase Service", Version = "v1" });
            });

            var app = builder.Build();

            // Always enable swagger
            app.UseSwagger();
            app.UseSwaggerUI();
            // REMOVED UseHttpsRedirection
            app.UseAuthorization();
            app.MapControllers();
            app.UseGlobalExceptionHandling();
            app.UseRequestLogging();
            app.Run();
        }
    }
}
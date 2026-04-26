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

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<PurchaseDBContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("PurchaseDBConnectionString"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null
                        );
                    }
                ));
            builder.Services.AddScoped<PurchaseRepository>();
            builder.Services.AddHttpClient();
            builder.Services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options =>
            {
                var address = builder.Configuration.GetValue<string>("Grpc:ProductService");
                options.Address = new Uri(address);
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            
            app.UseGlobalExceptionHandling();
            app.UseRequestLogging();

            app.Run();
        }
    }
}

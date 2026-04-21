using Microsoft.EntityFrameworkCore;
using ProductMicroservices.Models;
using ProductMicroservices.Repository;
using SharedLibrary.Common;
namespace ProductMicroservices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<ProductDBContext>();
       
            builder.Services.AddTransient<ProductRepository>();
            builder.Services.AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.ReferenceHandler =
               System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
       });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Enable lazy-loading proxies for demonstrations. Requires Microsoft.EntityFrameworkCore.Proxies package.
            builder.Services.AddDbContext<ProductDBContext>(options =>
                options.UseLazyLoadingProxies()
                       .UseSqlServer(builder.Configuration.GetConnectionString("ProductDBConnectionString")));
       
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

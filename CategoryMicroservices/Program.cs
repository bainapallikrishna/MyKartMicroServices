using Microsoft.EntityFrameworkCore;
using CategoryMicroservices.Models;
using CategoryMicroservices.Repository;
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
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseGlobalExceptionHandling();
           app.UseRequestLogging();
            app.MapControllers();

            app.Run();

        }
    }
}
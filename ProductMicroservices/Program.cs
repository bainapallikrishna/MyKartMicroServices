using Microsoft.EntityFrameworkCore;
using ProductMicroservices.Grpc;
using ProductMicroservices.Models;
using ProductMicroservices.Repository;
using SharedLibrary.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDBContext>(options =>
    options.UseLazyLoadingProxies()
           .UseSqlServer(
               builder.Configuration.GetConnectionString("ProductDBConnectionString"),
               sqlOptions =>
               {
                   sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
               }));

builder.Services.AddTransient<ProductRepository>();
builder.Services.AddGrpc();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDBContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<ProductGrpcService>();

app.UseGlobalExceptionHandling();
app.UseRequestLogging();

app.Run();
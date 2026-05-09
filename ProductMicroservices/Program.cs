using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using ProductMicroservices.Grpc;
using ProductMicroservices.Models;
using ProductMicroservices.Repository;
using SharedLibrary.Common;

var builder = WebApplication.CreateBuilder(args);

// ✅ Environment-aware config loading
var env = builder.Environment.EnvironmentName;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<ProductDBContext>(options =>
    options.UseLazyLoadingProxies()
           .UseSqlServer(
               builder.Configuration.GetConnectionString("ProductDBConnectionString"),
               sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
           ));

builder.Services.AddTransient<ProductRepository>();
builder.Services.AddGrpc();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Product Service", Version = "v1" });
});

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
        var db = services.GetRequiredService<ProductDBContext>();
        db.Database.Migrate();
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
//app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<ProductGrpcService>();
app.Run();
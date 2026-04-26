var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Important: Use the gateway-proxied swagger.json paths (browser can reach these).
    c.SwaggerEndpoint("/product/swagger/v1/swagger.json", "ProductMicroservices");
    c.SwaggerEndpoint("/category/swagger/v1/swagger.json", "CategoryMicroservices");
    c.SwaggerEndpoint("/purchase/swagger/v1/swagger.json", "PurchaseMicroservices");
    c.SwaggerEndpoint("/user/swagger/v1/swagger.json", "UserMicroservices");
    c.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

// All other routes are proxied to the microservices
app.MapReverseProxy();

app.Run();

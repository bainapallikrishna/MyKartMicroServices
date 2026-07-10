using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using SharedLibrary.Common;
using System.IO;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


    
var templatePath = Path.Combine(builder.Environment.ContentRootPath, "ocelot.template.json");
var generatedPath = Path.Combine(builder.Environment.ContentRootPath, "ocelot.generated.json");
if (File.Exists(templatePath))
{
    var template = File.ReadAllText(templatePath);
    var map = new Dictionary<string, string>
    {
        ["CategoryHost"] = Environment.GetEnvironmentVariable("CATEGORY_HOST") ?? builder.Configuration["Services:Category:Host"] ?? "localhost",
        ["CategoryPort"] = Environment.GetEnvironmentVariable("CATEGORY_PORT") ?? builder.Configuration["Services:Category:Port"] ?? "5124",
        ["ProductHost"] = Environment.GetEnvironmentVariable("PRODUCT_HOST") ?? builder.Configuration["Services:Product:Host"] ?? "localhost",
        ["ProductPort"] = Environment.GetEnvironmentVariable("PRODUCT_PORT") ?? builder.Configuration["Services:Product:Port"] ?? "21464",
        ["UserHost"] = Environment.GetEnvironmentVariable("USER_HOST") ?? builder.Configuration["Services:User:Host"] ?? "localhost",
        ["UserPort"] = Environment.GetEnvironmentVariable("USER_PORT") ?? builder.Configuration["Services:User:Port"] ?? "35805",
        ["PurchaseHost"] = Environment.GetEnvironmentVariable("PURCHASE_HOST") ?? builder.Configuration["Services:Purchase:Host"] ?? "localhost",
        ["PurchasePort"] = Environment.GetEnvironmentVariable("PURCHASE_PORT") ?? builder.Configuration["Services:Purchase:Port"] ?? "47513",
        ["GatewayBaseUrl"] = Environment.GetEnvironmentVariable("GATEWAY_BASEURL") ?? builder.Configuration["Gateway:BaseUrl"] ?? "http://localhost:5048"
    };

    foreach (var kv in map)
    {
        template = template.Replace("${" + kv.Key + "}", kv.Value);
    }

    File.WriteAllText(generatedPath, template);
    builder.Configuration.AddJsonFile("ocelot.Development.json", optional: true, reloadOnChange: true);
}
else
{
    // Fall back to any existing environment-specific ocelot files
    builder.Configuration
        .AddJsonFile("ocelot.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json",
                      optional: true,
                      reloadOnChange: true);
}

// Removed AddMyKartCors() - no such extension exists in the SharedLibrary. Register CORS below instead.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string[] {}
        }
    });
});
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
// CORS - allow requests from browser clients (adjust for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddRedisCache(builder.Configuration);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AngularPolicy");
app.UseSharedLogging();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
});

app.UseWhen(context =>
    !context.Request.Path.StartsWithSegments("/swagger") &&
    !context.Request.Path.StartsWithSegments("/swagger/docs") &&
    !context.Request.Path.StartsWithSegments("/user/Auth", StringComparison.OrdinalIgnoreCase),
    appBuilder =>
    {
        appBuilder.UseAuthentication();
        appBuilder.UseAuthorization();
    }
);

await app.UseOcelot();
app.Run();

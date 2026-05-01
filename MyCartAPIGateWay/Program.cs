using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load ONLY one ocelot file — no merging, no duplicate keys
var env = builder.Environment.EnvironmentName;
var ocelotFile = File.Exists($"ocelot.{env}.json")
    ? $"ocelot.{env}.json"      // exists → use environment specific file
    : "ocelot.json";            // fallback → use base file

builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Gateway", Version = "v1" });
});

builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
});



await app.UseOcelot();

app.Run();
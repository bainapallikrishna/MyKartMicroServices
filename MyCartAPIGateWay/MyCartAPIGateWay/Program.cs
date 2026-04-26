using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot;
using MMLib.SwaggerForOcelot.DependencyInjection;

namespace MyCartAPIGateWay
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // load ocelot routes
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

            builder.Services.AddOcelot(builder.Configuration);
            builder.Services.AddSwaggerForOcelot(builder.Configuration);

            var app = builder.Build();

            // serve aggregated swagger UI
            app.UseSwaggerForOcelotUI(options =>
            {
                // default aggregated UI will be available at /swagger
                options.PathToSwaggerGenerator = "/swagger/docs";
            });

            // start the gateway
            await app.UseOcelot();

            app.Run();
        }
    }
}
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using UserMicroservices.Models;
using UserMicroservices.Repository;

using SharedLibrary.Common;
using SharedLibrary.CQRS;
using Microsoft.Extensions.Logging;

namespace UserMicroservices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var env = builder.Environment.EnvironmentName;
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();


            builder.Services.AddStructuredLogging(builder.Configuration, "UserMicroservice");

            builder.Services.AddControllers();

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "User Service", Version = "v1" });
                // Add JWT Authorization to Swagger
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


            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthorizationPropagationHandler>();
            builder.Services.AddHttpClient("PropagatingClient").AddHttpMessageHandler<AuthorizationPropagationHandler>();
            builder.Services.AddDbContext<UserDBContext>(options =>
               options.UseSqlServer(
                   builder.Configuration.GetConnectionString("UserDBConnectionString"),
                   sqlOptions => sqlOptions.EnableRetryOnFailure(
                       maxRetryCount: 5,
                       maxRetryDelay: TimeSpan.FromSeconds(30),
                       errorNumbersToAdd: null
                   )
               ));
            builder.Services.AddScoped<UserRepository>();

            builder.Services.AddInMemoryDispatcher();
      
            builder.Services.AddScoped<SharedLibrary.CQRS.IRequestHandler<UserMicroservices.CQRS.Commands.CreateUserCommand, bool>, UserMicroservices.CQRS.Handlers.CreateUserCommandHandler>();
            builder.Services.AddScoped<SharedLibrary.CQRS.IRequestHandler<UserMicroservices.CQRS.Queries.GetAllUsersQuery, System.Collections.Generic.List<UserMicroservices.Models.User>>, UserMicroservices.CQRS.Handlers.GetAllUsersQueryHandler>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });
            builder.Services.AddRedisCache(builder.Configuration);

            builder.Services.AddSingleton<SharedLibrary.Common.ICacheService, SharedLibrary.Common.CacheService>();
            var app = builder.Build();

   
            app.UseStructuredLogging();

        
            app.UseSharedLogging();

    
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<UserDBContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            app.UseGlobalExceptionHandling();
            app.UseRequestLogging();
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthorization();
     
            var dispatcher = app.Services.GetRequiredService<SharedLibrary.CQRS.InMemoryDispatcher>();
            dispatcher.RegisterHandler<UserMicroservices.CQRS.Commands.CreateUserCommand, bool, UserMicroservices.CQRS.Handlers.CreateUserCommandHandler>();
            dispatcher.RegisterHandler<UserMicroservices.CQRS.Queries.GetAllUsersQuery, System.Collections.Generic.List<UserMicroservices.Models.User>, UserMicroservices.CQRS.Handlers.GetAllUsersQueryHandler>();
            app.MapControllers();

            app.Run();
        }
    }
}
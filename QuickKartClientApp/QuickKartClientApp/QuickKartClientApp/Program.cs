using Polly;
using Polly.Extensions.Http;

namespace QuickKartClientApp
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
            //builder.Services.AddHttpClient("Product",httpClient =>
            //{
            //    httpClient.BaseAddress = new Uri("https://localhost:44342/");
            //});
            //builder.Services.AddHttpClient("Category", httpClient =>
            //{
            //    httpClient.BaseAddress = new Uri("https://localhost:44341/");
            //});
            var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .OrTransientHttpError()
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(3));
            //builder.Services.AddHttpClient("apiGatewayServices", httpClient =>
            //{
            //    httpClient.BaseAddress = new Uri("https://localhost:7152/");
            //}).AddPolicyHandler(retryPolicy);
            var circutBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
         .OrTransientHttpError()
         .CircuitBreakerAsync(2,TimeSpan.FromSeconds(5));
            builder.Services.AddHttpClient("apiGatewayServices", httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://localhost:7152/");
            }).AddPolicyHandler(circutBreakerPolicy)
            .AddPolicyHandler(retryPolicy);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
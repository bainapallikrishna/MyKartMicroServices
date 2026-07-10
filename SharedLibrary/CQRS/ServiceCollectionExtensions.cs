using Microsoft.Extensions.DependencyInjection;

namespace SharedLibrary.CQRS
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryDispatcher(this IServiceCollection services)
        {
            services.AddSingleton<InMemoryDispatcher>();
            services.AddSingleton<IDispatcher>(sp => sp.GetRequiredService<InMemoryDispatcher>());

            return services;
        }
    }
}

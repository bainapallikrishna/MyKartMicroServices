using Microsoft.Extensions.DependencyInjection;
using System;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Extension methods for registering caching functionality in the dependency injection container.
    /// </summary>
    public static class CachingExtensions
    {
        /// <summary>
        /// Registers the caching action filter globally for all controllers.
        /// Must be called after AddControllers() and AddRedisCache().
        /// </summary>
        public static IServiceCollection AddCachingFilter(this IServiceCollection services)
        {
            services.AddScoped<CachingActionFilter>();
            services.AddControllers(options =>
            {
                options.Filters.AddService<CachingActionFilter>();
            });

            return services;
        }
    }
}

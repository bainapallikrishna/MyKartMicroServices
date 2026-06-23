using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Common
{
    public static class   RedisExtensions
    {
        public static IServiceCollection AddRedisCache(
       this IServiceCollection services,
       IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration =
                    configuration.GetConnectionString("Redis");
                options.InstanceName = "MyKart";
            });

            return services;
        }
    }
}

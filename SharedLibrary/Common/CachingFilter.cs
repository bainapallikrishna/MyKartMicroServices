using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Action filter to handle CacheableAttribute and InvalidateCacheAttribute.
    /// </summary>
    public class CachingFilter : IAsyncActionFilter
    {
        private readonly ICacheService _cacheService;

        public CachingFilter(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (method == null)
            {
                await next();
                return;
            }

            var cacheable = method.MethodInfo.GetCustomAttributes(typeof(CacheableAttribute), true).FirstOrDefault() as CacheableAttribute;
            if (cacheable != null)
            {
                // Build key
                string key = cacheable.CacheKeyPrefix ?? $"{method.ControllerName}:{method.ActionName}";
                // include route/query arguments for uniqueness
                if (context.ActionArguments != null && context.ActionArguments.Count > 0)
                {
                    var argPart = string.Join(";", context.ActionArguments.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    key = $"{key}:{argPart}";
                }

                var cached = await _cacheService.GetAsync<object>(key);
                if (cached != null)
                {
                    context.Result = new JsonResult(cached);
                    return;
                }

                var executed = await next();
                if (executed.Result is ObjectResult orr)
                {
                    var value = orr.Value;
                    await _cacheService.SetAsync(key, value, TimeSpan.FromSeconds(cacheable.DurationInSeconds));
                }
                else if (executed.Result is JsonResult jr)
                {
                    await _cacheService.SetAsync(key, jr.Value, TimeSpan.FromSeconds(cacheable.DurationInSeconds));
                }

                return;
            }

            var invalidate = method.MethodInfo.GetCustomAttributes(typeof(InvalidateCacheAttribute), true).FirstOrDefault() as InvalidateCacheAttribute;
            if (invalidate != null)
            {
                var executed = await next();

                // After execution, invalidate patterns
                foreach (var pattern in invalidate.CacheKeyPatterns ?? Array.Empty<string>())
                {
                    try
                    {
                        await _cacheService.RemoveByPatternAsync(pattern);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                return;
            }

            await next();
        }
    }
}

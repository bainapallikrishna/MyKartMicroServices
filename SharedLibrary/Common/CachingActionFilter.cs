using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Action filter that implements caching logic for endpoints decorated with [Cacheable] and [InvalidateCache].
    /// </summary>
    public class CachingActionFilter : IAsyncActionFilter
    {
        private readonly ICacheService _cacheService;
        private readonly IDistributedCache _distributedCache;

        public CachingActionFilter(ICacheService cacheService, IDistributedCache distributedCache)
        {
            _cacheService = cacheService;
            _distributedCache = distributedCache;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.ActionDescriptor.GetType().GetProperty("MethodInfo")?.GetValue(context.ActionDescriptor) as System.Reflection.MethodInfo;

            if (method == null)
            {
                await next();
                return;
            }

            // Check if the action has [Cacheable] attribute
            var cacheableAttr = method.GetCustomAttributes(typeof(CacheableAttribute), false)
                .FirstOrDefault() as CacheableAttribute;

            // Check if the action has [InvalidateCache] attribute
            var invalidateCacheAttr = method.GetCustomAttributes(typeof(InvalidateCacheAttribute), false)
                .FirstOrDefault() as InvalidateCacheAttribute;

            // Handle cacheable GET requests
            if (cacheableAttr != null)
            {
                var cacheKey = GenerateCacheKey(context, cacheableAttr);
                var cachedResult = await _cacheService.GetAsync<object>(cacheKey);

                if (cachedResult != null)
                {
                    context.Result = new JsonResult(cachedResult);
                    return;
                }

                var resultContext = await next();

                // Cache the result if the action succeeded
                if (resultContext.Result is JsonResult jsonResult && jsonResult.Value != null)
                {
                    await _cacheService.SetAsync(cacheKey, jsonResult.Value, TimeSpan.FromSeconds(cacheableAttr.DurationInSeconds));
                }

                return;
            }

            // Handle cache invalidation for write operations
            if (invalidateCacheAttr != null)
            {
                var resultContext = await next();

                // Invalidate cache patterns after successful execution
                if (invalidateCacheAttr.CacheKeyPatterns.Length > 0)
                {
                    await InvalidateCachePatterns(invalidateCacheAttr.CacheKeyPatterns);
                }

                return;
            }

            // No caching attributes, proceed normally
            await next();
        }

        private string GenerateCacheKey(ActionExecutingContext context, CacheableAttribute attr)
        {
            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            var action = context.RouteData.Values["action"]?.ToString() ?? "";

            var parameters = new Dictionary<string, object>();
            foreach (var param in context.ActionArguments)
            {
                parameters[param.Key] = param.Value ?? "";
            }

            if (!string.IsNullOrEmpty(attr.CacheKeyPrefix))
            {
                return CacheKeyHelper.GenerateKey(attr.CacheKeyPrefix, parameters);
            }

            return CacheKeyHelper.GenerateKey(controller, action, parameters);
        }

        private async Task InvalidateCachePatterns(string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                await InvalidateCachePattern(pattern);
            }
        }

        private async Task InvalidateCachePattern(string pattern)
        {
            try
            {
                // For Redis, we would ideally use SCAN with pattern matching
                // For now, we'll try to remove common patterns
                if (pattern.EndsWith("*"))
                {
                    // This is a simplified approach - in production, you might want to use Redis SCAN command
                    var basePattern = pattern.Replace("*", "");

                    // Try to remove the base pattern key (works if it exists)
                    await _cacheService.RemoveAsync(basePattern);

                    // Also try variations that might exist
                    var variations = new[]
                    {
                        basePattern,
                        $"{basePattern}:all",
                        $"{basePattern}:list",
                        $"{basePattern}:get"
                    };

                    foreach (var variation in variations)
                    {
                        try
                        {
                            await _cacheService.RemoveAsync(variation);
                        }
                        catch
                        {
                            // Silently ignore errors during cache cleanup
                        }
                    }
                }
                else
                {
                    // Exact pattern match
                    await _cacheService.RemoveAsync(pattern);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - cache invalidation failure shouldn't break the app
                System.Diagnostics.Debug.WriteLine($"Cache invalidation failed for pattern '{pattern}': {ex.Message}");
            }
        }
    }
}

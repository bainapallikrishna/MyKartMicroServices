using System;
using System.Collections.Generic;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Marks an action method as a cache invalidator. Applied to POST, PUT, DELETE endpoints.
    /// Automatically invalidates related cache entries after execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InvalidateCacheAttribute : Attribute
    {
        /// <summary>
        /// Cache key patterns to invalidate. Supports wildcards.
        /// Example: "user:*", "product:*", "category:*"
        /// </summary>
        public string[] CacheKeyPatterns { get; set; }

        public InvalidateCacheAttribute(params string[] cacheKeyPatterns)
        {
            CacheKeyPatterns = cacheKeyPatterns ?? Array.Empty<string>();
        }
        public string? CacheKeyPrefix { get; set; }
    }
}

using System;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Marks an action method as cacheable. Applied to GET endpoints.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheableAttribute : Attribute
    {
        /// <summary>
        /// Duration in seconds for which the result should be cached.
        /// Default is 300 seconds (5 minutes).
        /// </summary>
        public int DurationInSeconds { get; set; } = 300;

        /// <summary>
        /// Optional cache key prefix. If not provided, the controller and action names are used.
        /// </summary>
        public string? CacheKeyPrefix { get; set; }

        public CacheableAttribute(int durationInSeconds = 300)
        {
            DurationInSeconds = durationInSeconds;
        }
    }
}

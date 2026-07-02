using System;
using System.Collections.Generic;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Helper class for generating and managing cache keys consistently across the application.
    /// </summary>
    public static class CacheKeyHelper
    {
        private const string CacheKeyPrefix = "mykart";

        /// <summary>
        /// Generates a cache key based on controller, action, and parameters.
        /// </summary>
        public static string GenerateKey(string controller, string action, Dictionary<string, object>? parameters = null)
        {
            var key = $"{CacheKeyPrefix}:{controller.ToLower()}:{action.ToLower()}";

            if (parameters != null && parameters.Count > 0)
            {
                var paramString = string.Join(":", parameters.Values);
                key += $":{paramString}";
            }

            return key;
        }

        /// <summary>
        /// Generates a cache key with a custom prefix.
        /// </summary>
        public static string GenerateKey(string customPrefix, Dictionary<string, object>? parameters = null)
        {
            var key = $"{CacheKeyPrefix}:{customPrefix.ToLower()}";

            if (parameters != null && parameters.Count > 0)
            {
                var paramString = string.Join(":", parameters.Values);
                key += $":{paramString}";
            }

            return key;
        }

        /// <summary>
        /// Generates a simple key with just a resource name.
        /// </summary>
        public static string GenerateKey(string resourceName)
        {
            return $"{CacheKeyPrefix}:{resourceName.ToLower()}";
        }

        /// <summary>
        /// Gets the matching pattern for wildcard invalidation.
        /// </summary>
        public static string GetPatternKey(string pattern)
        {
            if (pattern.EndsWith("*"))
            {
                return pattern.Replace("*", "");
            }
            return pattern;
        }
    }
}

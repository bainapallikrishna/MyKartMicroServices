using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Common
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan expiry);

        Task RemoveAsync(string key);
        /// <summary>
        /// Remove cache entries that match the provided pattern. Supports '*' wildcard.
        /// </summary>
        /// <param name="pattern">Pattern to match (e.g. "product:*").</param>
        Task RemoveByPatternAsync(string pattern);
    }
}

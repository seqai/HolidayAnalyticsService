using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

// From https://stackoverflow.com/questions/31458950/is-there-any-guidance-for-caching-patterns-in-asp-net-5
namespace HolidayAnalyticsService.Infrastructure.DistributedCache
{
    public static class DistributedCacheExtensions
    {
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
        {
            return SetAsync(cache, key, value, new DistributedCacheEntryOptions());
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, value);
                bytes = memoryStream.ToArray();
            }

            return cache.SetAsync(key, bytes, options);
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var val = await cache.GetAsync(key);
            var result = default(T);

            if (val == null) return result;

            await using (var memoryStream = new MemoryStream(val))
            {
                var binaryFormatter = new BinaryFormatter();
                result = (T)binaryFormatter.Deserialize(memoryStream);
            }

            return result;
        }
    }
}

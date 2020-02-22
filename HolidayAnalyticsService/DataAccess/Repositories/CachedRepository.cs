using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using HolidayAnalyticsService.Infrastructure.DistributedCache;
using LanguageExt;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using static LanguageExt.Prelude;

namespace HolidayAnalyticsService.DataAccess.Repositories
{
    public class CachedRepository<T, TId> : IReadRepository<T, TId> where TId : IEquatable<TId>
    {
        private readonly IReadRepository<T, TId> _repository;
        private readonly IDistributedCache _cache;
        private readonly Func<TId, string> _keyFunc;
        private readonly Func<T, TId> _idFunc;
        private double _expiration;
        private readonly ILogger _logger;

        public CachedRepository(
            IReadRepository<T, TId> repository,
            IDistributedCache cache,
            Func<TId, string> keyFunc,
            Func<T, TId> idFunc,
            double expiration,
            ILogger logger)
        {
            _repository = repository;
            _cache = cache;
            _keyFunc = keyFunc;
            _idFunc = idFunc;
            _expiration = expiration;
            _logger = logger;
        }

        public TryOptionAsync<T> GetByIdAsync(TId id) => 
            GetFromCache(id).Plus(_repository.GetByIdAsync(id).Do(async x => await Cache(id, x)));

        public TryAsync<IEnumerable<T>> GetByIdsAsync(IEnumerable<TId> ids) => async () =>
        {
            var idList = ids.ToImmutableList();
            var cacheTasks = idList.Map(async id => (id, value: await GetFromCache(id).Try()));
            var cacheResult = await Task.WhenAll(cacheTasks);
            var missingIds = cacheResult.Where(x => x.value.IsFaultedOrNone).Select(x => x.id);
            var missingValuesTask = _repository.GetByIdsAsync(missingIds).Do(async xs =>
                await Task.WhenAll(xs.Map(async x => await Cache(_idFunc(x), x))));
            var missingValues = await missingValuesTask.Try();
            return missingValues.Map(xs =>
            {
                var valuesDict = cacheResult
                    .Map(x => x.value.Match(Some, () => None, e => None))
                    .Somes().Concat(xs)
                    .ToDictionary(x => _idFunc(x), x => x);
                return idList.Map(id => valuesDict.TryGetValue(id, out var value) ? Some(value) : None).Somes();
            });
        };

        private TryOptionAsync<T> GetFromCache(TId id) => async () =>
        {
            var key = _keyFunc(id);
            try
            {
                var cachedValue = await _cache.GetAsync<T>(key);
                if (cachedValue != null)
                {
                    _logger.Debug($"Got value from cache for key {key}");
                    return Some(cachedValue);
                }

                return None;
            }
            catch (Exception e)
            {
                _logger.Warning(e, $"Failed getting cache value for key: {key}");
                throw;
            }
        };

        private async Task Cache(TId id, T value)
        {
            var key = _keyFunc(id);
            try
            {
                await _cache.SetAsync<T>(key, value, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_expiration)
                });
                _logger.Debug($"Set value into cache for key {key}");
            }
            catch (Exception e)
            {
                _logger.Warning(e, $"Failed setting cache value for key: {key}");
            }
        }
    }
}
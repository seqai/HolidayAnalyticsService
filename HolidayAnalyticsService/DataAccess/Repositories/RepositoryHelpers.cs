using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace HolidayAnalyticsService.DataAccess.Repositories
{
    internal static class RepositoryHelpers
    {
        // Semaphore to control concurrency is the most straightforward out-of-the-box solution 
        public static Func<IEnumerable<TId>, TryAsync<IEnumerable<T>>> CreateGetByIds<T, TId>
        (
            Func<TId, TryOptionAsync<T>> get,
            Func<IEnumerable<TId>, Exception> exceptionFactory,
            int parallelism
        ) => (IEnumerable<TId> ids) => async () =>
        {
            var semaphore = new SemaphoreSlim(parallelism);
            var tasks = Task.WhenAll(ids.Map(async id =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Either should fail graciously providing missing id or ignore missing
                    return (id, result: await get(id).Match(
                        Some,
                        () => None,
                        e => throw e
                    ));
                }
                finally
                {
                    semaphore.Release();
                }
            }));
            var completed = await tasks;
            var missing = completed.Filter(x => x.result.IsNone).Map(x => x.id).ToImmutableList();
            if (missing.Any())
            {
                // Possible improvement: using Either on repository level for business error handling
                throw exceptionFactory(missing);
            }

            return new Result<IEnumerable<T>>(completed.Map(x => x.result).Somes());
        };
    }
}

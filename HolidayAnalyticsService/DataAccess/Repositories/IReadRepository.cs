using System.Collections.Generic;
using LanguageExt;

namespace HolidayAnalyticsService.DataAccess.Repositories
{
    public interface IReadRepository<T, in TId>
    {
        TryOptionAsync<T> GetByIdAsync(TId id);
        TryAsync<IEnumerable<T>> GetByIdsAsync(IEnumerable<TId> id);
    }
}

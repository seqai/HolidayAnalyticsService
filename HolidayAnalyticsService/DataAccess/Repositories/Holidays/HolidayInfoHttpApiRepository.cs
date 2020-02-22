using System.Collections.Generic;
using System.Linq;
using HolidayAnalyticsService.Infrastructure.Configuration;
using HolidayAnalyticsService.Infrastructure.HttpClients;
using HolidayAnalyticsService.Infrastructure.HttpClients.Exceptions;
using HolidayAnalyticsService.Model.Holidays;
using LanguageExt;
using Microsoft.Extensions.Options;
using static HolidayAnalyticsService.DataAccess.Repositories.RepositoryHelpers;

namespace HolidayAnalyticsService.DataAccess.Repositories.Holidays
{
    public class HolidayInfoHttpApiRepository : IReadRepository<HolidayInfo, HolidayInfoId>
    {
        private readonly HolidaysApiClient _client;
        private readonly ApiConfiguration _apiConfiguration;

        public HolidayInfoHttpApiRepository(HolidaysApiClient client, IOptions<ApiConfiguration> apiConfiguration)
        {
            _client = client;
            _apiConfiguration = apiConfiguration.Value;
        }

        public TryOptionAsync<HolidayInfo> GetByIdAsync(HolidayInfoId id) =>
            _client.GetHolidays(id.Year, id.CountryCode).Map(xs => new HolidayInfo(id, xs.ToList()));

        public TryAsync<IEnumerable<HolidayInfo>> GetByIdsAsync(IEnumerable<HolidayInfoId> ids) => 
            CreateGetByIds<HolidayInfo, HolidayInfoId>(
                GetByIdAsync,
                missing => new NoHolidayDataException(missing),
                _apiConfiguration.ParallelRequestsPerClient
            )(ids);


    }
}
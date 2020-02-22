using System;
using System.Collections.Generic;
using HolidayAnalyticsService.Infrastructure.Configuration;
using HolidayAnalyticsService.Infrastructure.HttpClients;
using HolidayAnalyticsService.Infrastructure.HttpClients.Exceptions;
using HolidayAnalyticsService.Model.Country;
using LanguageExt;
using Microsoft.Extensions.Options;
using static HolidayAnalyticsService.DataAccess.Repositories.RepositoryHelpers;

namespace HolidayAnalyticsService.DataAccess.Repositories.Holidays
{
    public class CountryInfoHttpApiRepository : IReadRepository<Country, string>
    {
        private readonly CountryDataApiClient _client;
        private readonly ApiConfiguration _apiConfigutation;

        public CountryInfoHttpApiRepository(CountryDataApiClient client, IOptions<ApiConfiguration> apiConfiguration)
        {
            _client = client;
            _apiConfigutation = apiConfiguration.Value;
        }

        public TryOptionAsync<Country> GetByIdAsync(string id) =>
            _client.GetCountryData(id);

        public TryAsync<IEnumerable<Country>> GetByIdsAsync(IEnumerable<string> ids) => 
            CreateGetByIds<Country, string>(
                GetByIdAsync,
                missing => new NoCountryDataException(missing), 
                _apiConfigutation.ParallelRequestsPerClient
            )(ids);


    }
}
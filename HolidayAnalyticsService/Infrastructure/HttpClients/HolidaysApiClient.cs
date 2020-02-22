using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using HolidayAnalyticsService.Infrastructure.JsonConverters;
using HolidayAnalyticsService.Model.Holidays;
using LanguageExt;
using static LanguageExt.Prelude;

namespace HolidayAnalyticsService.Infrastructure.HttpClients
{
    public class HolidaysApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public HolidaysApiClient(HttpClient client)
        {
            _httpClient = client;
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            _options.Converters.Add(new DateTimeOffsetTimeAgnosticJsonConverter());
            _options.Converters.Add(new OptionJsonConverterFactory());
            _options.Converters.Add(new CommaSeparatedFlags<PublicHolidayType>());
        }

        public TryOptionAsync<IEnumerable<Holiday>> GetHolidays(int year, string countryCode) => async () =>
            await _httpClient.GetAsync($"{year}/{countryCode}").MapAsync(async response =>
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return None;

                response.EnsureSuccessStatusCode();
                await using var responseStream = await response.Content.ReadAsStreamAsync();
                var holidays = await JsonSerializer.DeserializeAsync<IEnumerable<Holiday>>(responseStream, _options);
                return Some(holidays);
            });
    }
}
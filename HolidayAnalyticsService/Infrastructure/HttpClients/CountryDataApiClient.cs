using System.Net;
using System.Net.Http;
using System.Text.Json;
using HolidayAnalyticsService.Model.Country;
using LanguageExt;
using static LanguageExt.Prelude;

namespace HolidayAnalyticsService.Infrastructure.HttpClients
{
    public class CountryDataApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public CountryDataApiClient(HttpClient client)
        {
            _httpClient = client;
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

        }
        public TryOptionAsync<Country> GetCountryData(string countryCode) => async () =>
            await _httpClient.GetAsync(countryCode).MapAsync(async response =>
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return None;

                response.EnsureSuccessStatusCode();
                await using var responseStream = await response.Content.ReadAsStreamAsync();
                var country= await JsonSerializer.DeserializeAsync<Country>(responseStream, _options);
                return Some(country);
            });
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using HolidayAnalyticsService.Business.Errors;
using HolidayAnalyticsService.DataAccess.Repositories;
using HolidayAnalyticsService.DataAccess.Repositories.Holidays;
using HolidayAnalyticsService.Infrastructure.HttpClients.Exceptions;
using HolidayAnalyticsService.Model.Country;
using HolidayAnalyticsService.Model.Holidays;
using LanguageExt;
using Serilog;
using static LanguageExt.Prelude;

namespace HolidayAnalyticsService.Business.Holidays
{
    public class HolidaysCalculationFacade
    {
        private readonly IReadRepository<HolidayInfo, HolidayInfoId> _holidayRepository;
        private readonly IReadRepository<Country, string> _countryRepository;

        private readonly ILogger _logger;

        public HolidaysCalculationFacade(
            IReadRepository<HolidayInfo, HolidayInfoId> holidayRepository,
            IReadRepository<Country, string> countryRepository,
            ILogger logger
        ) {
            _holidayRepository = holidayRepository;
            _countryRepository = countryRepository;
            _logger = logger;
        }

        public EitherAsync<IBusinessError, IReadOnlyCollection<Holiday>> CalculateLongestSequence(int year,
            IEnumerable<string> countryCodes) =>
            CalculateLongestSequence(CreateHolidayInfo(year, countryCodes).ToImmutableList());

        private EitherAsync<IBusinessError, IReadOnlyCollection<Holiday>> CalculateLongestSequence(
            IImmutableList<HolidayInfoId> ids
        ) {
            var holidaysTask = _holidayRepository.GetByIdsAsync(ids)
                .Match(
                    x => Right<IBusinessError, IReadOnlyCollection<HolidayInfo>>(x.ToImmutableList()),
                    e => e switch
                    {
                        NoHolidayDataException noHolidayData => NoSuchItemError<IReadOnlyCollection<HolidayInfo>>(
                            noHolidayData.Message),
                        _ => LogServerError<IReadOnlyCollection<HolidayInfo>>(e)
                    }
                ).ToAsync();
            
            var countriesTask = _countryRepository.GetByIdsAsync(ids.Select(x => x.CountryCode))
                .Match(
                    x => Right<IBusinessError, IReadOnlyCollection<Country>>(x.ToImmutableList()),
                    e => e switch
                    {
                        NoCountryDataException noCountryData => NoSuchItemError<IReadOnlyCollection<Country>>(
                            noCountryData.Message),
                        _ => LogServerError<IReadOnlyCollection<Country>>(e)
                    }
                ).ToAsync();


            var segments = 
                from holidays in holidaysTask 
                from countries in countriesTask 
                select CreateSegments<bool>(holidays, countries);

            return segments;
        }

        private IReadOnlyCollection<Holiday> CreateSegments<U>(
            IReadOnlyCollection<HolidayInfo> holidays,
            IReadOnlyCollection<Country> countries
        )
        {
            return holidays.SelectMany(x => x.Holidays).ToImmutableList();
        }

        private static IEnumerable<HolidayInfoId> CreateHolidayInfo(int year, IEnumerable<string> countryCodes) =>
            countryCodes.Map(code => HolidayInfoId.Create(year, code))
                .Somes();

        private static Either<IBusinessError, T> NoSuchItemError<T>(string message) =>
            Left<IBusinessError, T>(new NoSuchItemError(message));

        private Either<IBusinessError, T> LogServerError<T>(Exception exception)
        {
            _logger.Error(exception, "Repository exception");
            return Left<IBusinessError, T>(new ServerError());
        }
    }
}
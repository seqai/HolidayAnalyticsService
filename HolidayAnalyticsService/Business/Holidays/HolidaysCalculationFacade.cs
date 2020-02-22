using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HolidayAnalyticsService.Business.Errors;
using HolidayAnalyticsService.DataAccess.Repositories;
using HolidayAnalyticsService.Infrastructure.HttpClients.Exceptions;
using HolidayAnalyticsService.Model.Country;
using HolidayAnalyticsService.Model.Holidays;
using LanguageExt;
using Serilog;
using static LanguageExt.Prelude;
using static HolidayAnalyticsService.Business.Holidays.HolidaysCalculation;

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
        )
        {
            _holidayRepository = holidayRepository;
            _countryRepository = countryRepository;
            _logger = logger;
        }

        public EitherAsync<IBusinessError, HolidaySegmentInfo> GetLongestSequence(int year,
            IEnumerable<string> countryCodes, bool optimize) =>
            GetLongestSequence(CreateHolidayInfo(year, countryCodes).ToImmutableList(), optimize);

        private EitherAsync<IBusinessError, HolidaySegmentInfo> GetLongestSequence(IImmutableList<HolidayInfoId> ids, bool optimize)
        {
            var holidaysTask = GetHolidayInfo(ids);
            var countriesTask = GetCountries(ids);

            var segments =
                from holidays in holidaysTask
                from countries in countriesTask
                select CalculateLongestSequence(holidays, countries, optimize);

            return segments.Map(x => new HolidaySegmentInfo(x.ToImmutableList()));
        }


        private EitherAsync<IBusinessError, IReadOnlyCollection<Country>> GetCountries(
            IImmutableList<HolidayInfoId> ids) =>
            _countryRepository.GetByIdsAsync(ids.Select(x => x.CountryCode))
                .Match(
                    x => Right<IBusinessError, IReadOnlyCollection<Country>>(x.ToImmutableList()),
                    e => e switch
                    {
                        NoCountryDataException noCountryData => NoSuchItemError<IReadOnlyCollection<Country>>(
                            noCountryData.Message),
                        _ => LogServerError<IReadOnlyCollection<Country>>(e)
                    }
                ).ToAsync();

        private EitherAsync<IBusinessError, IReadOnlyCollection<HolidayInfo>> GetHolidayInfo(
            IImmutableList<HolidayInfoId> ids) =>
            _holidayRepository.GetByIdsAsync(ids)
                .Match(
                    x => Right<IBusinessError, IReadOnlyCollection<HolidayInfo>>(x.ToImmutableList()),
                    e => e switch
                    {
                        NoHolidayDataException noHolidayData => NoSuchItemError<IReadOnlyCollection<HolidayInfo>>(
                            noHolidayData.Message),
                        _ => LogServerError<IReadOnlyCollection<HolidayInfo>>(e)
                    }
                ).ToAsync();

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
using Autofac;
using HolidayAnalyticsService.Business.Holidays;
using HolidayAnalyticsService.DataAccess.Repositories;
using HolidayAnalyticsService.DataAccess.Repositories.Holidays;
using HolidayAnalyticsService.Infrastructure.Configuration;
using HolidayAnalyticsService.Model.Country;
using HolidayAnalyticsService.Model.Holidays;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace HolidayAnalyticsService.Infrastructure
{
    public class ApiCoreModule : Module
    {
        private readonly CacheSettings _configuration;

        public ApiCoreModule(CacheSettings configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HolidaysCalculationFacade>().AsSelf();
            if (_configuration.Type == CacheType.None)
            {
                builder.RegisterType<HolidayInfoHttpApiRepository>().AsImplementedInterfaces();
                builder.RegisterType<CountryInfoHttpApiRepository>().AsImplementedInterfaces();
            }
            else
            {
                RegisterCache(builder);
            }
        }

        private void RegisterCache(ContainerBuilder builder)
        {
            builder.RegisterType<HolidayInfoHttpApiRepository>().AsSelf();
            builder.RegisterType<CountryInfoHttpApiRepository>().AsSelf();

            builder.Register(context =>
            {
                var repository = context.Resolve<HolidayInfoHttpApiRepository>();
                var cache = context.Resolve<IDistributedCache>();
                var logger = context.Resolve<ILogger>();

                return new CachedRepository<HolidayInfo, HolidayInfoId>(
                    repository,
                    cache,
                    id => $"{_configuration.HolidaysPrefix}:{id.Year}{id.CountryCode.ToUpper()}",
                    info => info.Id,
                    _configuration.LifetimeHours,
                    logger
                );
            }).As<IReadRepository<HolidayInfo, HolidayInfoId>>();

            builder.Register(context =>
            {
                var repository = context.Resolve<CountryInfoHttpApiRepository>();
                var cache = context.Resolve<IDistributedCache>();
                var logger = context.Resolve<ILogger>();

                return new CachedRepository<Country, string>(
                    repository,
                    cache,
                    id => $"{_configuration.CountryPrefix}:{id.ToUpper()}",
                    x => x.Alpha2Code.ToUpper(),
                    _configuration.LifetimeHours,
                    logger
                );
            }).As<IReadRepository<Country, string>>();

        }
    }
}

namespace HolidayAnalyticsService.Infrastructure.Configuration
{
    public class CacheSettings
    {
        public CacheType Type { get; set; }
        public string ConfigurationString { get; set; }
        public string HolidaysPrefix { get; set; }
        public string CountryPrefix { get; set; }
        public string RedisInstanceName { get; set; }
        public double LifetimeHours { get; set; } = 0.1;
    }
}

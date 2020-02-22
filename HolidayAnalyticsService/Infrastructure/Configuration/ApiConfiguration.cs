namespace HolidayAnalyticsService.Infrastructure.Configuration
{
    public class ApiConfiguration
    {
        public string HolidaysUri { get; set; }
        public string CountryDataUri { get; set; }
        public string UserAgent { get; set; }
        public int ParallelRequestsPerClient { get; set; } = 5;
    }
}

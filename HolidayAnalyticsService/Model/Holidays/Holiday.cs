using System;
using LanguageExt;

namespace HolidayAnalyticsService.Model.Holidays
{
    [Serializable]
    public class Holiday
    { 
        public DateTimeOffset Date { get; set; }
        public string LocalName { get; set; }
        public string Name { get; set; }
        public bool Fixed { get; set; }
        public bool Global { get; set; }
        public PublicHolidayType Type { get; set; }
        public string CountryCode { get; set; }
        public Option<int> LaunchYear { get; set; }
    }
}

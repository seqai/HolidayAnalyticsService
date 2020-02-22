using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayAnalyticsService.Model.Country
{
    [Serializable]
    public class Country
    {
        public string Name { get; set; }
        public string Alpha2Code { get; set; }
        public List<string> Timezones { get; set; } = new List<string>();
    }
}

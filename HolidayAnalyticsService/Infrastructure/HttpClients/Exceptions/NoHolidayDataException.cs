using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolidayAnalyticsService.Model.Holidays;

namespace HolidayAnalyticsService.Infrastructure.HttpClients.Exceptions
{
    public class NoHolidayDataException : Exception
    {
        public NoHolidayDataException(IEnumerable<HolidayInfoId> ids) : base(
            $"No holiday information for: {string.Join(", ", ids.Select(x => $"country '{x.CountryCode}' year '{x.Year}'"))}"
        ) { }
    }
}

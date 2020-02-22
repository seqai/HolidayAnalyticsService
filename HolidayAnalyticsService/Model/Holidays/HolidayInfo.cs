using System;
using System.Collections.Generic;

namespace HolidayAnalyticsService.Model.Holidays
{
    [Serializable]
    public class HolidayInfo
    {
        public HolidayInfo(HolidayInfoId id, IReadOnlyCollection<Holiday> holidays)
        {
            Id = id;
            Holidays = holidays;
        }

        public HolidayInfoId Id { get; }
        public IReadOnlyCollection<Holiday> Holidays { get; }
    }
}

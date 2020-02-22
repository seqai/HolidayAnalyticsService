using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayAnalyticsService.Model.Holidays
{
    public class HolidaySegment
    {
        public DateTimeOffset Start { get;  }
        public DateTimeOffset End { get;  }
        public DateTimeOffset StartUtc => Start.UtcDateTime;
        public DateTimeOffset EndUtc => End.UtcDateTime;
        public long StartEpoch => Start.ToUnixTimeMilliseconds();
        public long EndEpoch => End.ToUnixTimeMilliseconds();
        public Holiday Holiday { get;  }
        public string Timezone { get;  }
        public HolidaySegment(DateTimeOffset start, DateTimeOffset end, Holiday holiday, string timezone)
        {
            Start = start;
            End = end;
            Holiday = holiday;
            Timezone = timezone;
        }
    }
}

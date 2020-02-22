using System;
using System.Collections.Immutable;
using System.Linq;

namespace HolidayAnalyticsService.Model.Holidays
{
    public class HolidaySegmentInfo
    {
        public HolidaySegmentInfo(ImmutableList<HolidaySegment> segments)
        {
            Segments = segments;
        }
        public int Total => Segments.Count;
        public DateTimeOffset Start => Segments.Select(x => x.Start).FirstOrDefault();
        public DateTimeOffset End => Segments.Select(x => x.End).FirstOrDefault();
        public DateTimeOffset StartUtc => Start.UtcDateTime;
        public DateTimeOffset EndUtc => End.UtcDateTime;

        public ImmutableList<string> Details =>
            Segments.Select(x => $"{x.Holiday.CountryCode} {x.Holiday.Name} {x.Holiday.Date:yyyy-MM-dd} {x.Timezone}")
                .ToImmutableList();

        public ImmutableList<HolidaySegment> Segments { get; }

    }
}

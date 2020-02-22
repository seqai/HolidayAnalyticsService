using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using HolidayAnalyticsService.Model.Country;
using HolidayAnalyticsService.Model.Holidays;
using LanguageExt;
using static LanguageExt.Prelude;

namespace HolidayAnalyticsService.Business.Holidays
{
    internal static class HolidaysCalculation
    {
        public static IReadOnlyCollection<HolidaySegment> CalculateLongestSequence(
            IReadOnlyCollection<HolidayInfo> holidays,
            IReadOnlyCollection<Country> countries,
            bool optimize = true
        )
        {
            var timezones = countries.ToDictionary(x => x.Alpha2Code, x => GenerateTimezones(x.Timezones), StringComparer.InvariantCultureIgnoreCase);

            var segments = from holidayInfo in holidays
                from holiday in holidayInfo.Holidays
                where (holiday.Type & PublicHolidayType.Public) != 0
                from timezone in timezones.TryGetValue(holidayInfo.Id.CountryCode, out var v) ? Some(v) : None
                from segment in CreateSegments(holiday, timezone)
                orderby segment.Start
                select segment;

            // Imperative algorithm

            var start = DateTimeOffset.MinValue;
            var end = DateTimeOffset.MinValue;

            var currentMax = TimeSpan.Zero;
            var maxSegments = new List<HolidaySegment>();
            var currentSegments = new List<HolidaySegment>();

            foreach (var segment in segments)
            {
                if (segment.Start > end)
                {
                    var newMax = end - start;
                    if (newMax > currentMax)
                    {
                        maxSegments = currentSegments;
                        currentMax = newMax;
                    }
                    currentSegments = new List<HolidaySegment> { segment };
                    start = segment.Start;
                    end = segment.End;
                }
                else
                {
                    currentSegments.Add(segment);
                    end = segment.End;
                }
            }

            // Optionally we may want to throw out reduntant segments
            // It can also be done during the first run as we may achieve better performance
            var length = maxSegments.Count;

            if (!optimize || length <= 2) return maxSegments;

            var optimizedSegments = new List<HolidaySegment> { maxSegments[0] };
            for (var i = 1; i < length - 1; i++)
            {
                if (maxSegments[i + 1].Start > optimizedSegments.Last().End)
                    optimizedSegments.Add(maxSegments[i]);
            }
            optimizedSegments.Add(maxSegments[length - 1]);

            return optimizedSegments;
        }

        public static IEnumerable<HolidaySegment> CreateSegments(Holiday holiday, ImmutableList<TimeZoneInfo> tzs)
        {
            return tzs.Map(tz => new HolidaySegment(
                new DateTimeOffset(holiday.Date.DateTime, tz.BaseUtcOffset),     
                new DateTimeOffset(holiday.Date.DateTime, tz.BaseUtcOffset).AddDays(1),     
                holiday
            ));
        }

        public static ImmutableList<TimeZoneInfo> GenerateTimezones(IEnumerable<string> offsets) =>
            offsets.Map(StringToOffset).Somes().Map(offset => TimeZoneInfo.CreateCustomTimeZone(
                offset.code,
                new TimeSpan(offset.hours, offset.minutes, 0),
                offset.code,
                offset.code
            )).ToImmutableList();

        private static Option<(string code, int hours, int minutes)> StringToOffset(string offset)
        {
            if (offset == "UTC")
            {
                return (offset, 0, 0);
            }

            // Regex can be replaced with a loop for a bit better efficiency and less readability
            var regex = new Regex(@"^UTC([+-]\d\d):(\d\d)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = regex.Matches(offset);
            if (matches.Count == 0)
            {
                return None;
            }

            var match = matches[0];
            if (int.TryParse(match.Groups[1].Value, out var hours) &&
                int.TryParse(match.Groups[2].Value, out var minutes))
            {
                return Some((offset, hours, minutes));
            }

            return None;
        }
    }
}

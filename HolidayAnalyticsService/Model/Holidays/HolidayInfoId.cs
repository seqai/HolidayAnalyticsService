using System;
using LanguageExt;
using static LanguageExt.Prelude;

namespace HolidayAnalyticsService.Model.Holidays
{
    [Serializable]
    public struct HolidayInfoId : IEquatable<HolidayInfoId>
    {
        private HolidayInfoId(int year, string countryCode)
        {
            Year = year;
            CountryCode = countryCode;
        }

        public int Year { get; }
        public string CountryCode { get; }

        public bool Equals(HolidayInfoId other)
        {
            return Year == other.Year && CountryCode.ToUpper() == other.CountryCode.ToUpper();
        }

        public override bool Equals(object obj)
        {
            return obj is HolidayInfoId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Year * 397) ^ (CountryCode != null ? CountryCode.ToUpper().GetHashCode() : 0);
            }
        }

        /// <summary>
        ///     Creates a holiday calendar id of full year and two-letter country code
        /// </summary>
        /// <param name="year">Calendar full year number</param>
        /// <param name="countryCode">ISO 3166-1 alpha-2 two-letter country code</param>
        public static Option<HolidayInfoId> Create(int year, string countryCode) => 
            countryCode?.Length == 2 ? Some(new HolidayInfoId(year, countryCode)) : None;
    }
}

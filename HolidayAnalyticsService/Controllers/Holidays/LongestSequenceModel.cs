using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HolidayAnalyticsService.Controllers.Holidays
{
    public class LongestSequenceModel
    {
        private static readonly ImmutableList<string> DefaultCountries = new List<string>
        {
            "AD", "AR", "AT", "AU", "AX", "BB", "BE", "BG", "BO", "BR", "BS", "BW", "BY", "BZ", "CA", "CH",
            "CL", "CN", "CO", "CR", "CU", "CY", "CZ", "DE", "DK", "DO", "EC", "EE", "EG", "ES", "FI", "FO",
            "FR", "GA", "GB", "GD", "GL", "GR", "GT", "GY", "HN", "HR", "HT", "HU", "IE", "IM", "IS", "IT",
            "JE", "JM", "LI", "LS", "LT", "LU", "LV", "MA", "MC", "MD", "MG", "MK", "MT", "MX", "MZ", "NA",
            "NI", "NL", "NO", "NZ", "PA", "PE", "PL", "PR", "PT", "PY", "RO", "RS", "RU", "SE", "SI", "SJ",
            "SK", "SM", "SR", "SV", "TN", "TR", "UA", "US", "UY", "VA", "VE", "ZA"
        }.ToImmutableList();

        private List<string> _countries;

        [FromRoute] [Required] public int Year { get; set; }

        [FromQuery(Name = "Country")]
        public List<string> Countries
        {
            get => _countries?.Count > 0 ? _countries : new List<string>(DefaultCountries);
            set => _countries = value;
        }
    }
}
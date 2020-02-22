﻿using System.Threading.Tasks;
using HolidayAnalyticsService.Business.Errors;
using HolidayAnalyticsService.Business.Holidays;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace HolidayAnalyticsService.Controllers.Holidays
{
    [ApiController]
    [Route("[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly HolidaysCalculationFacade _calculation;

        public HolidaysController(HolidaysCalculationFacade calculation)
        {
            _calculation = calculation;
        }

        [HttpGet]
        [Route("longest-sequence/{Year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> LongestSequence([FromQuery] LongestSequenceModel model) =>
            _calculation.CalculateLongestSequence(model.Year, model.Countries).Match(Ok, MatchError);

        private IActionResult MatchError(IBusinessError error) => error switch
        {
            NoSuchItemError e => NotFound(e.Message),
            _ => StatusCode(500) as IActionResult
        };
    }
} 
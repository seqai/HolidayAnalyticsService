using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayAnalyticsService.Business.Errors
{
    public interface IBusinessError
    {
        string Message { get; }
    }
}

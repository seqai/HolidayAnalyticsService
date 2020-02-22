using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayAnalyticsService.Business.Errors
{
    public class ServerError : IBusinessError
    {
        public ServerError(string message = "Internal Server Error")
        {
            Message = message;
        }

        public string Message { get; }
    }
}

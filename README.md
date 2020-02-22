# HolidayAnalyticsService
Sample .NET Core Web Api Service using language-ext and consuming external http services with Redis/InMemory cache

## Running the Service

The service can be run either using .NET Core runtime directly or in Docker

### Prerequisites

- .NET Core 3.1
- Redis (optional)
- Docker (optional)

### Dotnet runtime

Simply run
```
dotnet run
```
Service implements caching of http api data, by default in-memory cache is used. Optionally Redis might be used as distributed cache. Http configuration and caching is configurable.
Adjust appsettings.json or provide environment variables. Service will run on default ports 5000 and 5001 for TLS. Automatic HTTPS redirect is turned off.

### Docker

To build and run locally run with superuser privileges

```
docker build -f HolidayAnalyticsService/Dockerfile -t holiday-web-service .
docker run -p 7777:80 holiday-web-service
```

This will route service HTTP endpoints to port 7777

### Working with the service

Navigate to [swagger/index.html](http://localhost:5000/swagger/index.html) to use Swagger UI or use `/holidays/longest-sequence/{Year}` endpoint directly

### Implementation notes

- Actual calculation of the longest holiday sequence is a very non-trivial task, as anything concerning time, dates and timezones. Provided API for timezones doesn't include any daylight saving time information, as well as historical timezones. [Noda Time](https://nodatime.org/) library might be used to address DSTs, but historical calculation still will be a challenge. Also it seems that information is missing on restcountries.eu, e.g. Russia is missing UTC+2 Timezone (Kaliningrad) 
- Very simple algorithm is used which iterates through the sorted list of time segments, where each segment represent a holiday in a single timezone `O(nlogn)`
- Optional optimization of result by removing redundant segments
- Time spent on travel with the speed of light is ignored, holiday assumed to end precisely at the beginning on the day after

Main application architecture ideas:
- Use classic layered architecture and separating related layer classes into entities, business-objects and DTOs
- Use language-ext to promote more declarative and type-safe style, especially avoiding `null`s
- Additional industry-standard libraries were used: Autofac as IoC-container and Serilog to manage logging 
- Using cache to reduce load on external APIs

### Further development and problems out of the scope

- Taking DST into account
- All code put in the single project to keep it simple. Real-life scalable solution will probably have separate projects for api, data-access layer, business orchestration, etc
- Configuration management kept simple and minimal 
- Unit Testing for business logic and repositories


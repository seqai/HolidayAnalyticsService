using System;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacSerilogIntegration;
using HolidayAnalyticsService.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using HolidayAnalyticsService.Infrastructure;
using HolidayAnalyticsService.Infrastructure.HttpClients;
using HolidayAnalyticsService.Infrastructure.JsonConverters;
using Serilog;

namespace HolidayAnalyticsService
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables("HOLIDAYS_SERVICE_");
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }
        public CacheSettings CacheSettings { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var apiConfigurationSection = Configuration.GetSection(ConfigurationSections.ExternalApis);
            var apiConfiguration = apiConfigurationSection.Get<ApiConfiguration>();
            CacheSettings = Configuration.GetSection(ConfigurationSections.CacheSettings).Get<CacheSettings>();
            services.Configure<ApiConfiguration>(apiConfigurationSection);

            if (CacheSettings.Type == CacheType.Memory) 
                services.AddDistributedMemoryCache();
            else if (CacheSettings.Type == CacheType.Redis)
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = CacheSettings.ConfigurationString;
                    options.InstanceName = CacheSettings.RedisInstanceName;
                });

                services.AddOptions();
            services.AddControllers()
                .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        options.JsonSerializerOptions.Converters.Add(new OptionJsonConverterFactory());
                    });
            
            services.AddHttpClient<HolidaysApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiConfiguration.HolidaysUri);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd(apiConfiguration.UserAgent);
            });

            services.AddHttpClient<CountryDataApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiConfiguration.CountryDataUri);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd(apiConfiguration.UserAgent);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Holiday Analytics Web Api", Version = "v1" });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new ApiCoreModule(CacheSettings));
            builder.RegisterLogger();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Holiday Analytics Web Api");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }

    }
}

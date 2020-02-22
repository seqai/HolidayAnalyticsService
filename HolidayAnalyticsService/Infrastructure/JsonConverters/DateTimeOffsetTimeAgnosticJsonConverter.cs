using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HolidayAnalyticsService.Infrastructure.JsonConverters
{
    public class DateTimeOffsetTimeAgnosticJsonConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.Parse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }
}

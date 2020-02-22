using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HolidayAnalyticsService.Infrastructure.JsonConverters
{
    public class CommaSeparatedFlags<T> : JsonConverter<T> where T : struct, Enum, IComparable
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return (T) (object) reader.GetString().Split(',').Select(x => Enum.TryParse<T>(
                    x.Trim(), out var parsed ) ? (int) (object) parsed : 0).Sum();
            }

            return default(T);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

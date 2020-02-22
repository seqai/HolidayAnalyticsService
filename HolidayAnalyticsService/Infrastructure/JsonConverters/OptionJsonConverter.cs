using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using static LanguageExt.Prelude;

namespace HolidayAnalyticsService.Infrastructure.JsonConverters
{
    public class OptionJsonConverter<T> : JsonConverter<Option<T>>
    {
        public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var converter = GetKeyConverter(options);
            if (reader.TokenType == JsonTokenType.Null)
            {
                return None;
            }
            var key = converter.Read(ref reader, typeToConvert, options);
            return Optional(key);
        }

        public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
        {

            var converter = GetKeyConverter(options);
            value.Match(
                x => converter.Write(writer, x, options),
                writer.WriteNullValue
            );
        }

        private static JsonConverter<T> GetKeyConverter(JsonSerializerOptions options)
        {
            var converter = options.GetConverter(typeof(T)) as JsonConverter<T>;

            if (converter is null)
                throw new JsonException($"No JSON converter for type {typeof(T).Name}");

            return converter;
        }
    }
}

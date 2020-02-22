using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;

namespace HolidayAnalyticsService.Infrastructure.JsonConverters
{
    public class OptionJsonConverterFactory  : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            var type = typeToConvert;

            if (!type.IsGenericTypeDefinition)
                type = type.GetGenericTypeDefinition();

            return type == typeof(Option<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert,
            JsonSerializerOptions options)
        {
            var keyType = typeToConvert.GenericTypeArguments[0];
            var converterType = typeof(OptionJsonConverter<>).MakeGenericType(keyType);

            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }
}

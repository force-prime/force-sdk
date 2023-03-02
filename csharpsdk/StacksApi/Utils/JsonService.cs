using System;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StacksForce.Utils
{
    public static class JsonService
    {
        public static readonly JsonSerializerOptions SERIALIZER_OPTIONS =
            new JsonSerializerOptions { IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, NumberHandling = JsonNumberHandling.AllowReadingFromString  };

        private static readonly JsonSerializerOptions SERIALIZER_OPTIONS_PRETTY =
            new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, IncludeFields = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };

        static JsonService()
        {
            SERIALIZER_OPTIONS.Converters.Add(new BigIntegerConverter());
            SERIALIZER_OPTIONS_PRETTY.Converters.Add(new BigIntegerConverter());
        }

        static public string Serialize(object obj, Type type, bool pretty = false)
        {
            var options = pretty ? SERIALIZER_OPTIONS_PRETTY : SERIALIZER_OPTIONS;
            return JsonSerializer.Serialize(obj, type, options);
        }

        static public string Serialize<T>(T obj, bool pretty = false)
        {
            var options = pretty ? SERIALIZER_OPTIONS_PRETTY : SERIALIZER_OPTIONS;
            return JsonSerializer.Serialize(obj, options);
        }

        static public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, SERIALIZER_OPTIONS);
        }

        public class BigIntegerConverter : JsonConverter<BigInteger>
        {
            public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.Number)
                    throw new JsonException(string.Format("Found token {0} but expected token {1}", reader.TokenType, JsonTokenType.Number));
                using var doc = JsonDocument.ParseValue(ref reader);
                return BigInteger.Parse(doc.RootElement.GetRawText(), NumberFormatInfo.InvariantInfo);
            }

            public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options) =>
                writer.WriteRawValue(value.ToString(NumberFormatInfo.InvariantInfo), false);
        }
    }
}

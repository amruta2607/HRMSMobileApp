using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// JSON converter for Location Tracking timestamps.
    /// Format: yyyy-MM-ddTHH:mm:ss (no milliseconds, no timezone/offset).
    /// </summary>
    public sealed class LocationTrackingTimestampJsonConverter : JsonConverter<DateTime>
    {
        public const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                return default;
            }

            var raw = reader.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return default;
            }

            // Reject milliseconds / timezone suffix / offset.
            // Examples to reject: 2026-07-07T09:20:04.610Z, 2026-07-07T09:20:04Z, 2026-07-07T09:20:04+05:30
            if (raw.Contains('.') || raw.EndsWith('Z') || raw.Contains('+') || raw.Contains('-') && raw.Length > 19)
            {
                return default;
            }

            if (DateTime.TryParseExact(
                    raw,
                    TimestampFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                // Unspecified kind: mobile sends local (IST) without an offset.
                return DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Always emit without milliseconds / timezone.
            writer.WriteStringValue(value.ToString(TimestampFormat, CultureInfo.InvariantCulture));
        }
    }

    public sealed class NullableLocationTrackingTimestampJsonConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var parsed = new LocationTrackingTimestampJsonConverter().Read(ref reader, typeof(DateTime), options);
            return parsed == default ? null : parsed;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Value.ToString(LocationTrackingTimestampJsonConverter.TimestampFormat, CultureInfo.InvariantCulture));
        }
    }
}


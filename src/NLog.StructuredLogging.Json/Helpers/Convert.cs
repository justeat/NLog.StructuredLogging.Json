using System;
using System.Globalization;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class Convert
    {
        public static string ToUtcIso8601(DateTimeOffset source)
        {
            return string.Concat(source.ToUniversalTime().ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff", CultureInfo.InvariantCulture), "Z");
        }

        public static string ValueAsString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is DateTime)
            {
                return DateTimeValueAsIso8601String((DateTime)value);
            }

            if (value is DateTimeOffset)
            {
                return DateTimeOffsetValueAsIso8601String((DateTimeOffset)value);
            }

            return value.ToString();
        }

        private static string DateTimeOffsetValueAsIso8601String(DateTimeOffset value)
        {
            if (value.Offset == TimeSpan.Zero)
            {
                // do this to make it end in "Z" not "+00:00"
                return DateTimeValueAsIso8601String(value.UtcDateTime);
            }

            return value.ToString("O", CultureInfo.InvariantCulture);
        }

        private static string DateTimeValueAsIso8601String(DateTime value)
        {
            return value.ToString("O", CultureInfo.InvariantCulture);
        }
    }
}

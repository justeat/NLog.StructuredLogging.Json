using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class Mapper
    {
        public static Dictionary<string, object> ToDictionary(LogEventInfo source)
        {
            var timestampUtcIso8601 = Convert.ToUtcIso8601(source.TimeStamp);

            var result = new Dictionary<string, object>
            {
                {"TimeStamp", timestampUtcIso8601 },
                {"Level", source.Level.ToString()},
                {"LoggerName", source.LoggerName}
            };

            var hasTemplate = !string.Equals(source.Message, source.FormattedMessage, StringComparison.Ordinal);

            if (hasTemplate)
            {
                result.Add("Message", source.FormattedMessage);
                result.Add("MessageTemplate", source.Message);
            }
            else
            {
                result.Add("Message", source.Message);
            }

            if (source.Exception != null)
            {
                result.Add("Exception", source.Exception.ToString());
                result.Add("ExceptionType", source.Exception.GetType().Name);
                result.Add("ExceptionMessage", source.Exception.Message);
                result.Add("ExceptionStackTrace", source.Exception.StackTrace);
                result.Add("ExceptionFingerprint", ConvertException.ToFingerprint(source.Exception));
            }

            if (!hasTemplate && source.Parameters != null)
            {
                result.Add("Parameters", string.Join(",", source.Parameters.Select(Convert.ValueAsString)));
            }

            if (source.StackTrace != null)
            {
                result.Add("CallSite", StackHelper.CallSiteName(source.StackTrace));
            }

            HarvestToDictionary(source.Properties, result, "data_");

            if (source.Exception != null)
            {
                HarvestToDictionary(source.Exception.Data, result, "ex_");
            }

            return result;
        }

        public static void HarvestToDictionary(
        IDictionary<object, object> source,
        IDictionary<string, object> dest,
        string keyPrefixWhenCollision)
        {
            if (source == null)
            {
                return;
            }

            foreach (var property in source)
            {
                if (property.Value.IsNonStringValueType())
                {
                    HarvestToDictionary(dest, property.Key.ToString(), property.Value, keyPrefixWhenCollision);
                }
                else
                {
                    var valueAsString = Convert.ValueAsString(property.Value);
                    HarvestStringToDictionary(dest, property.Key.ToString(), valueAsString, keyPrefixWhenCollision);
                }
            }
        }

        public static void HarvestToDictionary(IDictionary source, IDictionary<string, object> dest, string keyPrefixWhenCollision)
        {
            if (source == null)
            {
                return;
            }

            foreach (DictionaryEntry property in source)
            {
                if (property.Value.IsNonStringValueType())
                {
                    HarvestToDictionary(dest, property.Key.ToString(), property.Value, keyPrefixWhenCollision);
                }
                else
                {
                    var valueAsString = Convert.ValueAsString(property.Value);
                    HarvestStringToDictionary(dest, property.Key.ToString(), valueAsString, keyPrefixWhenCollision);
                }
            }
        }

        private static void HarvestToDictionary(
            IDictionary<string, object> dest,
            string key,
            object value,
            string keyPrefixWhenCollision)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            key = SafeCharsInKey(key);

            if (!dest.ContainsKey(key))
            {
                dest.Add(key, value);
                return;
            }

            var prefixedKey = keyPrefixWhenCollision + key;
            if (!dest.ContainsKey(prefixedKey))
            {
                dest.Add(prefixedKey, value);
            }
        }

        public static void HarvestStringToDictionary(
            IDictionary<string, object> dest,
            string key,
            string value,
            string keyPrefixWhenCollision)
        {
            HarvestToDictionary(dest, key, value, keyPrefixWhenCollision);
        }

        private static string SafeCharsInKey(string rawKey)
        {
            return rawKey.Replace('.', '_');
        }
    }
}


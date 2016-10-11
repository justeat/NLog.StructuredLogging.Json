using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                {"LoggerName", source.LoggerName},
                {"Message", source.FormattedMessage}
            };

            if (source.Exception != null)
            {
                result.Add("Exception", source.Exception.ToString());
                result.Add("ExceptionType", source.Exception.GetType().Name);
                result.Add("ExceptionMessage", source.Exception.Message);
                result.Add("ExceptionStackTrace", source.Exception.StackTrace);
                result.Add("ExceptionFingerprint", ConvertException.ToFingerprint(source.Exception));
            }

            if (source.Parameters != null)
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

        public static void HarvestToDictionary(IDictionary<object, object> source, IDictionary<string, object> result, string keyPrefixWhenCollision)
        {
            if (source == null)
            {
                return;
            }

            foreach (var property in source)
            {
                HarvestToDictionary(property.Key.ToString(), property.Value, result, keyPrefixWhenCollision);
            }
        }

        public static void HarvestToDictionary(IDictionary source, IDictionary<string, object> result, string keyPrefixWhenCollision)
        {
            if (source == null)
            {
                return;
            }

            foreach (DictionaryEntry property in source)
            {
                HarvestToDictionary(property.Key.ToString(), property.Value, result, keyPrefixWhenCollision);
            }
        }

        private static void HarvestToDictionary(string key, object value, IDictionary<string, object> result, string keyPrefixWhenCollision)
        {
            var valueAsString = Convert.ValueAsString(value);
            if (!result.ContainsKey(key))
            {
                result.Add(key, valueAsString);
            }
            else
            {
                result.Add(keyPrefixWhenCollision+key, valueAsString);
            }
        }
    }
}

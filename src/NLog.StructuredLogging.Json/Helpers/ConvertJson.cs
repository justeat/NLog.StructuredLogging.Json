using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class ConvertJson
    {
        internal static readonly JsonSerializerSettings LogSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            Formatting = Formatting.None
        };

        internal static JsonSerializer CreateJsonSerializer() => JsonSerializer.CreateDefault(LogSettings);

        public static string Serialize(Dictionary<string, object> data)
        {
            return JsonConvert.SerializeObject(data, LogSettings);
        }

        public static void Serialize(Dictionary<string, object> data, System.Text.StringBuilder sb, JsonSerializer jsonSerializer)
        {
            using (var sw = new StringWriter(sb))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    jsonSerializer.Serialize(writer, data);
                }
            }
        }
    }
}

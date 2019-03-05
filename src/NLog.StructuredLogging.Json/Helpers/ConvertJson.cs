using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class ConvertJson
    {
        private static readonly JsonSerializerSettings LogSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            Formatting = Formatting.None
        };

        public static string Serialize(Dictionary<string, object> data)
        {
            return JsonConvert.SerializeObject(data, LogSettings);
        }
    }
}

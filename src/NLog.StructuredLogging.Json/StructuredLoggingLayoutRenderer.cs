using System.Text;
using NLog.Config;
using NLog.StructuredLogging.Json.Helpers;
using NLog.LayoutRenderers;
using Newtonsoft.Json;

namespace NLog.StructuredLogging.Json
{
    [LayoutRenderer("structuredlogging.json")]
    [ThreadSafe]
    public class StructuredLoggingLayoutRenderer : LayoutRenderer
    {
        private JsonSerializer JsonSerializer => _jsonSerializer ?? (_jsonSerializer = ConvertJson.CreateJsonSerializer());
        private JsonSerializer _jsonSerializer;

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var dictionary = Mapper.ToDictionary(logEvent);

            var orgLength = builder.Length;

            try
            {
                // Ensure we are threadsafe
                var jsonSerializer = JsonSerializer;
                lock (jsonSerializer)
                {
                    // Serialize directly into StringBuilder
                    ConvertJson.Serialize(dictionary, builder, jsonSerializer);
                }
            }
            catch
            {
                _jsonSerializer = null; // Do not reuse JsonSerializer, as it might have become broken
                builder.Length = orgLength;  // Skip invalid JSON
                throw;
            }
        }

        protected override void CloseLayoutRenderer()
        {
            _jsonSerializer = null;
            base.CloseLayoutRenderer();
        }
    }
}

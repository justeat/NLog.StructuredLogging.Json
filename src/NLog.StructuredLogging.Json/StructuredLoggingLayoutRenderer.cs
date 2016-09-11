using System.Text;
using NLog.StructuredLogging.Json.Helpers;
using NLog.LayoutRenderers;

namespace NLog.StructuredLogging.Json
{
    [LayoutRenderer("structuredlogging.json")]
    public class StructuredLoggingLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var dictionary = Mapper.ToDictionary(logEvent);
            var json = ConvertJson.Serialize(dictionary);
            builder.Append(json);
        }
    }
}

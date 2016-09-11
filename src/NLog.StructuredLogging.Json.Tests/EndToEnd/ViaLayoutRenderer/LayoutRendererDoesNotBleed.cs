using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class LayoutRendererDoesNotBleed : DynamicLogPropertiesDoNotBleedToSubsequentLogEvents
    {
        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}

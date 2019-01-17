using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class UnicodePropertiesInLayoutRenderer : UnicodePropertiesAreSerialised
    {
        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}
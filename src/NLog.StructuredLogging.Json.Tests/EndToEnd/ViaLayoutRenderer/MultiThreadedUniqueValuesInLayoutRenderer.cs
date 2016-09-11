using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class MultiThreadedUniqueValuesInLayoutRenderer : MultiThreadedUniqueValuesTests
    {
        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}

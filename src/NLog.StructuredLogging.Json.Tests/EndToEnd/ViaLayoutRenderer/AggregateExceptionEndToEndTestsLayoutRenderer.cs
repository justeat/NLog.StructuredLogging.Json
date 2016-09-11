using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class AggregateExceptionEndToEndTestsLayoutRenderer : AggregateExceptionEndToEndTests
    {
        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}

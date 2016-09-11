using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class AggregateExceptionEndToEndTestsInFlattenedJson : AggregateExceptionEndToEndTests
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }
}

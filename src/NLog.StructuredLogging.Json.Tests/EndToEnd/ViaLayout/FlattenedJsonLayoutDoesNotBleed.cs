using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class FlattenedJsonLayoutDoesNotBleed : DynamicLogPropertiesDoNotBleedToSubsequentLogEvents
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }
}
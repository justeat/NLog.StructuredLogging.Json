using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class DateTimePropertiesInFlattenedJson : DateTimePropertiesAreSerialised
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }
}
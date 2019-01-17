using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class UnicodePropertiesInFlattenedJson : UnicodePropertiesAreSerialised
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }
}
using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class MultiUniqueKeysThreadedInFlattenedJson : MultiThreadedUniqueKeysTests
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }
}
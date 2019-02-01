using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class MessageTemplateInLayoutRenderer : MessageTemplateTest
    {
        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}

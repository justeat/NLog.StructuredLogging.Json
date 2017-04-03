using System;
using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.JsonWithProperties
{
    public class FailingLayout : Layout
    {
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            throw new LoggingException("Test render fail");
        }
    }
}

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

    public class NullReferenceLayout : Layout
    {
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            throw new NullReferenceException("Test render null ref");
        }
    }
}

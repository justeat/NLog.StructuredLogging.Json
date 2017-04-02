using System;

namespace NLog.StructuredLogging.Json.Tests
{
    public class LoggingException : Exception
    {
        public LoggingException()
        {
        }

        public LoggingException(string message) : base(message)
        {
        }
    }
}

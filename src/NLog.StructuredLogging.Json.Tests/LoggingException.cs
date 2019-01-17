using System;

namespace NLog.StructuredLogging.Json.Tests
{
    [Serializable]
    public class LoggingException : Exception
    {
        public LoggingException()
        {
        }

        public LoggingException(string message) : base(message)
        {
        }

        public LoggingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

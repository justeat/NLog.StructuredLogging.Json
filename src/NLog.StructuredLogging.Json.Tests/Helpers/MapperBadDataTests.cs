using System;
using NLog.StructuredLogging.Json.Helpers;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    [TestFixture]
    public class MapperBadDataTests
    {
        [Test]
        public void CanCopeWithVariousParams()
        {
            var logEventInfo = new LogEventInfo
            {
                Exception = null,
                Level = LogLevel.Error,
                LoggerName = "ExampleLoggerName",
                Message = "Example Message",
                TimeStamp = new DateTime(2014, 1, 2, 3, 4, 5, 6),
                Parameters = new object[] { 123, 34.4d, "sometext", "" }
            };

            var dict = Mapper.ToDictionary(logEventInfo);

            Assert.That(dict, Is.Not.Empty);
        }
        [Test]
        public void CanCopeWithNullParams()
        {
            var logEventInfo = new LogEventInfo
            {
                Exception = null,
                Level = LogLevel.Error,
                LoggerName = "ExampleLoggerName",
                Message = "Example Message",
                TimeStamp = new DateTime(2014, 1, 2, 3, 4, 5, 6),
                Parameters = new object[] { "sometext", null }
            };

            var dict = Mapper.ToDictionary(logEventInfo);

            Assert.That(dict, Is.Not.Empty);
        }
    }
}

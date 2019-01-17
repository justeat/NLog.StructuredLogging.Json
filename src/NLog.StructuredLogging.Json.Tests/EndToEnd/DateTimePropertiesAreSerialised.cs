using System;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    public abstract class DateTimePropertiesAreSerialised : EndToEndTests
    {
        private string _output;

        protected override void When()
        {
            var logInfo = new
                {
                    DateTimeInLocal = new DateTime(2014, 1, 2, 3, 4, 5, DateTimeKind.Local),
                    DateTimeInUtc = new DateTime(2014, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                    DateTimeOffsetInUtc = new DateTimeOffset(new DateTime(2014, 1, 2, 3, 4, 5), TimeSpan.Zero),
                    DateTimeOffsetWithOffset = new DateTimeOffset(new DateTime(2014, 1, 2, 3, 4, 5), new TimeSpan(4, 0, 0))
               };

            Sut.ExtendedInfo("testMessage", logInfo);
            var lines = LogManager.Configuration.LogMessage(TargetName);
            _output = lines[0];
        }

        [Test]
        public void OutputHasExpectedIso8601Dates()
        {
            Assert.That(_output, Does.Contain("\"DateTimeInLocal\":\"2014-01-02T03:04:05"));
            Assert.That(_output, Does.Contain("\"DateTimeInUtc\":\"2014-01-02T03:04:05.0000000Z\""));
            Assert.That(_output, Does.Contain("\"DateTimeOffsetInUtc\":\"2014-01-02T03:04:05.0000000Z\""));
            Assert.That(_output, Does.Contain("\"DateTimeOffsetWithOffset\":\"2014-01-02T03:04:05.0000000+04:00\""));
        }
    }
}

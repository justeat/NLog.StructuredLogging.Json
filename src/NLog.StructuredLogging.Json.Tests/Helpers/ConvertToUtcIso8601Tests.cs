using System;
using NUnit.Framework;
using Convert = NLog.StructuredLogging.Json.Helpers.Convert;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    [TestFixture]
    public class ConvertToUtcIso8601Tests
    {
        [Test]
        public void CanSerialiseDateTimeOffset()
        {
            var dateTimeOffset = new DateTimeOffset(new DateTime(2014, 1, 31, 18, 45, 55, DateTimeKind.Utc));
            var serialised = Convert.ToUtcIso8601(dateTimeOffset);
            Assert.That(serialised, Is.EqualTo("2014-01-31T18:45:55.000Z"));
        }

        [Test]
        public void WhenSerialised_TheOffsetIsKept()
        {
            var dateTimeOffset = new DateTimeOffset(new DateTime(2014, 1, 2, 3, 4, 5, DateTimeKind.Utc).AddMilliseconds(123.456));
            var serialised = Convert.ToUtcIso8601(dateTimeOffset);
            Assert.That(serialised, Is.EqualTo("2014-01-02T03:04:05.123Z"));
        }
    }
}

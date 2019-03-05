using System;
using System.Collections.Generic;
using NLog.StructuredLogging.Json.Helpers;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    [TestFixture]
    public class MapperDateTimeFormatTests
    {
        [Test]
        public void LocalDateTimeIsSerialisedAsIso8601()
        {
            var dateValue = new DateTime(2016, 1, 30, 12, 15, 45);

            var results = MakeMappedValues("dateValue", dateValue);
            var result = results["dateValue"];

            Assert.That(result, Is.EqualTo("2016-01-30T12:15:45.0000000"));
        }

        [Test]
        public void UtcDateTimeIsSerialisedAsIso8601()
        {
            var dateValue = new DateTime(2016, 1, 30, 12, 15, 45, DateTimeKind.Utc);

            var results = MakeMappedValues("dateValue", dateValue);
            var result = results["dateValue"];

            Assert.That(result, Is.EqualTo("2016-01-30T12:15:45.0000000Z"));
        }

        [Test]
        public void NullableDateTimeIsSerialisedAsIso8601()
        {
            DateTime? dateValue = new DateTime(2016, 1, 30, 12, 15, 45);

            var results = MakeMappedValues("dateValue", dateValue);
            var result = results["dateValue"];

            Assert.That(result, Is.EqualTo("2016-01-30T12:15:45.0000000"));
        }

        [Test]
        public void NullDateTimeIsSerialised()
        {
            DateTime? dateValue = null;

            var results = MakeMappedValues("dateValue", dateValue);
            var result = results["dateValue"];

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DateTimeOffsetIsSerialisedAsIso8601()
        {
            var dateTimeOffset = new DateTimeOffset(2016, 1, 30, 12, 15, 45, new TimeSpan(1, 30, 0));

            var results = MakeMappedValues("dateValue", dateTimeOffset);
            var result = results["dateValue"];

            Assert.That(result, Is.EqualTo("2016-01-30T12:15:45.0000000+01:30"));
        }

        [Test]
        public void UtcDateTimeOffsetIsSerialisedAsIso8601()
        {
            var dateTimeOffset = new DateTimeOffset(2016, 1, 30, 12, 15, 45, TimeSpan.Zero)
                .ToUniversalTime();

            var results = MakeMappedValues("dateValue", dateTimeOffset);
            var result = results["dateValue"];

            Assert.That(result, Is.EqualTo("2016-01-30T12:15:45.0000000Z"));
        }

        [Test]
        public void NullableDateTimeOffsetIsSerialisedAsIso8601()
        {
            DateTimeOffset? dateTimeOffset = new DateTimeOffset(2016, 1, 30, 12, 15, 45, new TimeSpan(1, 30, 0));

            var results = MakeMappedValues("dateValue", dateTimeOffset);
            var result = results["dateValue"];

            Assert.That(result, Is.EqualTo("2016-01-30T12:15:45.0000000+01:30"));
        }

        [Test]
        public void NullDateTimeOffsetIsSerialised()
        {
            DateTimeOffset? dateTimeOffset = null;

            var results = MakeMappedValues("dateValue", dateTimeOffset);
            var result = results["dateValue"];

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void NullableUtcDateTimeOffsetIsSerialisedAsIso8601()
        {
            DateTimeOffset? dateTimeOffset = new DateTimeOffset(2016, 1, 30, 12, 15, 45, TimeSpan.Zero)
                .ToUniversalTime();

            var results = MakeMappedValues("dateValue", dateTimeOffset);
            var result = results["dateValue"];

            Assert.That(result, Is.EqualTo("2016-01-30T12:15:45.0000000Z"));
        }

        private Dictionary<string, object> MakeMappedValues(string key, object value)
        {
            var logEventInfo = MakeStandardLogEventInfo();
            logEventInfo.Properties.Add(key, value);

            return Mapper.ToDictionary(logEventInfo);
        }

        private LogEventInfo MakeStandardLogEventInfo()
        {
            return new LogEventInfo
            {
                Exception = null,
                Level = LogLevel.Error,
                LoggerName = "ExampleLoggerName",
                Message = "Example Message",
                TimeStamp = new DateTime(2014, 1, 2, 3, 4, 5, 6),
            };
        }
    }
}

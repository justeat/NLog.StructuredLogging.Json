using System;
using NUnit.Framework;
using Convert = NLog.StructuredLogging.Json.Helpers.Convert;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    [TestFixture]
    public class ConvertValueAsStringTests
    {
        [Test]
        public void CanSerialiseString()
        {
            var serialised = Convert.ValueAsString("hello, world");
            Assert.That(serialised, Is.EqualTo("hello, world"));
        }

        [Test]
        public void CanSerialiseInt()
        {
            var serialised = Convert.ValueAsString(42);
            Assert.That(serialised, Is.EqualTo("42"));
        }

        [Test]
        public void CanSerialiseNull()
        {
            var serialised = Convert.ValueAsString(null);
            Assert.That(serialised, Is.Empty);
        }

        [Test]
        public void WhenDateTimeIsSerialised_FormatIsIso8601()
        {
            var value = new DateTime(2014, 1, 31, 18, 45, 55);
            var serialised = Convert.ValueAsString(value);
            Assert.That(serialised, Is.EqualTo("2014-01-31T18:45:55.0000000"));
        }

        [Test]
        public void WhenUtcDateTimeIsSerialised_FormatIsIso8601_ZuluTime()
        {
            var value = new DateTime(2014, 1, 31, 18, 45, 55, DateTimeKind.Utc);
            var serialised = Convert.ValueAsString(value);
            Assert.That(serialised, Is.EqualTo("2014-01-31T18:45:55.0000000Z"));
        }

        [Test]
        public void WhenNullableDateTimeIsSerialised_FormatIsIso8601()
        {
            DateTime? value = new DateTime(2014, 1, 31, 18, 45, 55, DateTimeKind.Utc);
            var serialised = Convert.ValueAsString(value);
            Assert.That(serialised, Is.EqualTo("2014-01-31T18:45:55.0000000Z"));
        }

        [Test]
        public void WhenNullDateTimeIsSerialised_ResultIsEmpty()
        {
            DateTime? value = null;
            var serialised = Convert.ValueAsString(value);
            Assert.That(serialised, Is.Empty);
        }

        [Test]
        public void WhenUtcDateTimeOffsetIsSerialised_ResultIsIso8601()
        {
            var value = new DateTimeOffset(new DateTime(2014, 1, 31, 18, 45, 55, DateTimeKind.Utc));
            var serialised = Convert.ValueAsString(value);
            Assert.That(serialised, Is.EqualTo("2014-01-31T18:45:55.0000000Z"));
        }

        [Test]
        public void WhenDateTimeOffsetWithOffsetIsSerialised_ResultIsIso8601WithOffset()
        {
            var value = new DateTimeOffset(new DateTime(2014, 1, 31, 18, 45, 55), new TimeSpan(6, 30, 0));
            var serialised = Convert.ValueAsString(value);
            Assert.That(serialised, Is.EqualTo("2014-01-31T18:45:55.0000000+06:30"));
        }

        [Test]
        public void WhenDateTimeOffsetWithnegaticOffsetIsSerialised_ResultIsIso8601WithOffset()
        {
            var value = new DateTimeOffset(new DateTime(2014, 1, 31, 18, 45, 55), new TimeSpan(-4, 00, 0));
            var serialised = Convert.ValueAsString(value);
            Assert.That(serialised, Is.EqualTo("2014-01-31T18:45:55.0000000-04:00"));
        }

        [Test]
        public void WhenNullDateTimeOffsetIsSerialised_ResultIsEmpty()
        {
            DateTimeOffset? value = new DateTimeOffset(new DateTime(2014, 1, 31, 18, 45, 55, DateTimeKind.Utc));
            var serialised = Convert.ValueAsString(value);
            Assert.That(serialised, Is.EqualTo("2014-01-31T18:45:55.0000000Z"));
        }
    }
}

using System;
using System.Collections.Generic;
using NLog.StructuredLogging.Json.Helpers;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    [TestFixture]
    public class MapperParametersTests
    {
        [Test]
        public void NullParamsAreIgnored()
        {
            var eventDict = ToEventDictionary(null);

            Assert.That(eventDict, Is.Not.Empty);
            Assert.That(eventDict.ContainsKey("Parameters"), Is.False);
        }

        [Test]
        public void EmptyParamsAreParsed()
        {
            var input = new object[0];
            var actualParams = ProcessParams(input);

            Assert.That(actualParams, Is.Empty);
        }

        [Test]
        public void OneStringParamsIsParsed()
        {
            var input = new object[] { "foo" };
            var actualParams = ProcessParams(input);

            Assert.That(actualParams, Is.EqualTo("foo"));
        }

        [Test]
        public void TwoStringParamsAreParsed()
        {
            var input = new object[] { "foo", "bar" };
            var actualParams = ProcessParams(input);

            Assert.That(actualParams, Is.EqualTo("foo,bar"));
        }

        [Test]
        public void NullForParamsIsParsed()
        {
            var input = new object[] { null };
            var actualParams = ProcessParams(input);

            Assert.That(actualParams, Is.Empty);
        }

        [Test]
        public void NullInParamsIsParsed()
        {
            var input = new object[] { "foo", null, "bar" };
            var actualParams = ProcessParams(input);

            Assert.That(actualParams, Is.EqualTo("foo,,bar"));
        }

        [Test]
        public void MultipleParamTypesAreParsed()
        {
            var input = new object[] {123, 34.4d, "sometext", false, ""};
            var actualParams = ProcessParams(input);

            Assert.That(actualParams, Is.EqualTo("123,34.4,sometext,False,"));
        }

        [Test]
        public void NullInMultipleParamsIsParsed()
        {
            var input = new object[] { 123.45, null, "bar" };
            var actualParams = ProcessParams(input);

            Assert.That(actualParams, Is.EqualTo("123.45,,bar"));
        }

        [Test]
        public void DateTimeParamTypesAreParsed()
        {
            var input = new object[] { 123, new DateTime(2017, 04, 01, 12, 13, 14) };
            var actualParams = ProcessParams(input);

            Assert.That(actualParams, Is.EqualTo("123,2017-04-01T12:13:14.0000000"));
        }

        private string ProcessParams(object[] parameters)
        {
            var logEntryDict = ToEventDictionary(parameters);

            Assert.That(logEntryDict, Is.Not.Empty);
            Assert.That(logEntryDict.ContainsKey("Parameters"));

            return logEntryDict["Parameters"].ToString();
        }

        private static Dictionary<string, object> ToEventDictionary(object[] parameters)
        {
            var logEventInfo = new LogEventInfo
            {
                Exception = null,
                Level = LogLevel.Error,
                LoggerName = "ExampleLoggerName",
                Message = "Example Message",
                TimeStamp = new DateTime(2017, 1, 2, 3, 4, 5, 6),
                Parameters = parameters
            };

            return Mapper.ToDictionary(logEventInfo);
        }
    }
}

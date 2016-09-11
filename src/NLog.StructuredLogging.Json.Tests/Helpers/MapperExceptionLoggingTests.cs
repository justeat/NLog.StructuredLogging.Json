using System;
using NLog.StructuredLogging.Json.Helpers;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    /// <summary>
    /// Tests different scenarios related to logging exceptions and the exception stack trace
    /// </summary>
    [TestFixture]
    public class MapperExceptionLoggingTests
    {
        [Test]
        public void WhenConvertedWithoutException_TheRightNumberOfFieldsAreReturned()
        {
            var logEventInfo = MakeStandardLogEventInfo();
            logEventInfo.Exception = null;
            var result = Mapper.ToDictionary(logEventInfo);

            Assert.AreEqual(7, result.Count);
        }

        [Test]
        public void WhenConvertedWithException_TheRightNumberOfFieldsAreReturned()
        {
            var logEventInfo = MakeStandardLogEventInfo();
            logEventInfo.Exception = GenerateExceptionWithStackTrace();
            logEventInfo.Properties.Clear();

            var result = Mapper.ToDictionary(logEventInfo);

            Assert.AreEqual(10, result.Count);
        }

        [Test]
        public void WhenConverted_TheExceptionFieldsAndValuesAreLogged()
        {
            var logEventInfo = MakeStandardLogEventInfo();
            logEventInfo.Exception = GenerateExceptionWithStackTrace();
            logEventInfo.Properties.Clear();

            var result = Mapper.ToDictionary(logEventInfo);

            Assert.That(result["Exception"], Is.StringStarting("System.InvalidOperationException: A test exception with a stack trace"));
            Assert.That(result["ExceptionType"], Is.EqualTo("InvalidOperationException"));
            Assert.That(result["ExceptionMessage"], Is.EqualTo("A test exception with a stack trace"));
            Assert.That(result["ExceptionStackTrace"], Is.Not.Empty);
            Assert.That(result["ExceptionFingerprint"], Is.Not.Empty);
        }

        [Test]
        public void When_ExceptionHasStackTraceData_StackTraceIsLogged()
        {
            var logEventInfo = MakeStandardLogEventInfo();
            logEventInfo.Exception = GenerateExceptionWithStackTrace();
            logEventInfo.Properties.Clear();

            var result = Mapper.ToDictionary(logEventInfo);

            Assert.That(result.ContainsKey("ExceptionStackTrace"), Is.True);
            Assert.That(result["ExceptionStackTrace"], Is.StringStarting("   at NLog.StructuredLogging.Json.Tests.Helpers.MapperExceptionLoggingTests.GenerateExceptionWithStackTrace()"));
        }

        [Test]
        public void When_ExceptionHasStackTraceData_ExceptionFingerprintIsLoggedAsSha1()
        {
            var logEventInfo = MakeStandardLogEventInfo();
            logEventInfo.Exception = GenerateExceptionWithStackTrace();
            logEventInfo.Properties.Clear();

            var result = Mapper.ToDictionary(logEventInfo);
            var exceptionSha1 = result["ExceptionFingerprint"].ToString();

            Assert.That(exceptionSha1.Length, Is.EqualTo(40));
        }

        private static LogEventInfo MakeStandardLogEventInfo()
        {
            return new LogEventInfo
            {
                Exception = new Exception("Outer Exception", new Exception("Inner Exception")),
                Level = LogLevel.Error,
                LoggerName = "ExampleLoggerName",
                Message = "Example Message",
                Parameters = new object[] { "One", 1234 },
                Properties = { { "PropertyOne", "This value is in property one" }, { "PropertyTwo", 2 } },
                TimeStamp = new DateTime(2014, 1, 2, 3, 4, 5, 6),
            };
        }

        private static Exception GenerateExceptionWithStackTrace()
        {
            try
            {
                throw new InvalidOperationException("A test exception with a stack trace");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}

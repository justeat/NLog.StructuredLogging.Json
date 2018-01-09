using System;
using NLog.StructuredLogging.Json.Helpers;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    /// <summary>
    /// Tests different scenarios related to logging the exception.data dictionary
    /// </summary>
    [TestFixture]
    public class MapperExceptionDataTests
    {
        [Test]
        public void When_ExceptionHasNoData()
        {
            var logEventInfo = MakeLogEventInfoWithException(new Exception());
            var result = Mapper.ToDictionary(logEventInfo);

            Assert.That(result.ContainsKey("ExKey1"), Is.False);
            Assert.That(result.ContainsKey("ExKey2"), Is.False);
        }

        [Test]
        public void When_ExceptionDataIsConverted()
        {
            var logEventInfo = MakeLogEventInfoWithException(ExceptionWithData());
            var result = Mapper.ToDictionary(logEventInfo);

            Assert.That(result.ContainsKey("ExKey1"), Is.True);
            Assert.That(result.ContainsKey("ExKey2"), Is.True);
            Assert.That(result.ContainsKey("NoSuchKey"), Is.False);

            Assert.That(result["ExKey1"], Is.EqualTo("value1"));
            Assert.That(result["ExKey2"], Is.EqualTo("value2"));
        }

        [Test]
        public void When_ExceptionDataAndPropertiesClash()
        {
            var logEventInfo = MakeLogEventInfoWithException(ExceptionWithData());
            logEventInfo.Properties.Clear();
            logEventInfo.Properties.Add("ExKey1", "valueOverrridenInProperties");
            var result = Mapper.ToDictionary(logEventInfo);

            Assert.That(result["ExKey1"], Is.EqualTo("valueOverrridenInProperties"));
            Assert.That(result["ExKey2"], Is.EqualTo("value2"));
        }

        [Test]
        public void When_ExceptionDataKeyContainDots()
        {
            var logEventInfo = MakeLogEventInfoWithException(ExceptionWithData());
            logEventInfo.Properties.Clear();
            logEventInfo.Properties.Add("data.name1", "test1");
            var result = Mapper.ToDictionary(logEventInfo);

            Assert.That(result.ContainsKey("data.name1"), Is.False);
            Assert.That(result.ContainsKey("data_name1"), Is.True);

            Assert.That(result["data_name1"], Is.EqualTo("test1"));
        }

        [Test]
        public void When_ExceptionDataKeysContainDots()
        {
            var logEventInfo = MakeLogEventInfoWithException(ExceptionWithData());
            logEventInfo.Properties.Clear();
            logEventInfo.Properties.Add("data.name1", "test1");
            logEventInfo.Properties.Add("data.name2", "test2");

            var result = Mapper.ToDictionary(logEventInfo);

            Assert.That(result.ContainsKey("data.name1"), Is.False);
            Assert.That(result.ContainsKey("data.name2"), Is.False);
            Assert.That(result.ContainsKey("data_name1"), Is.True);
            Assert.That(result.ContainsKey("data_name2"), Is.True);

            Assert.That(result["data_name1"], Is.EqualTo("test1"));
            Assert.That(result["data_name2"], Is.EqualTo("test1"));
        }

        private static LogEventInfo MakeLogEventInfoWithException(Exception ex)
        {
            return new LogEventInfo
            {
                Exception = ex,
                Level = LogLevel.Error,
                LoggerName = "ExampleLoggerName",
                Message = "Example Message",
                TimeStamp = new DateTime(2014, 1, 2, 3, 4, 5, 6),
            };
        }

        private static Exception ExceptionWithData()
        {
            var ex = new Exception("Exception");
            ex.Data.Add("ExKey1", "value1");
            ex.Data.Add("ExKey2", "value2");
            return ex;
        }
    }
}

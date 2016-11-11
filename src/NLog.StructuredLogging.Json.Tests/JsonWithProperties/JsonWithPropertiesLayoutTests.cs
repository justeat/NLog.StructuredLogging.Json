using System;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Time;
using NUnit.Framework;
using Shouldly;

namespace NLog.StructuredLogging.Json.Tests.JsonWithProperties
{
    [TestFixture]
    public class JsonWithPropertiesLayoutTests
    {
        private const string LoggerName = "TestLoggerName";
        private const string TestMessage = "This is the test message.";

        private static class TestProperties
        {
            public const string One = "Property One";
            public const int Two = 2;
            public const bool Three = true;
        }

        [Test]
        public void PropertiesAreAppendedToJsonOutput()
        {
            const string targetName = "60DDC370-0F37-4072-B006-18C2FEBEC06F";

            var layout = new JsonWithPropertiesLayout();
            layout.Properties.Add(new StructuredLoggingProperty("One", new SimpleLayout(TestProperties.One)));
            layout.Properties.Add(new StructuredLoggingProperty("Two", new SimpleLayout(TestProperties.Two.ToString())));
            layout.Properties.Add(new StructuredLoggingProperty("Three", new SimpleLayout(TestProperties.Three.ToString())));

            var target = new MemoryTarget
            {
                Name = targetName,
                Layout = layout
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            TimeSource.Current = new FakeTimeSource();
            var logger = LogManager.GetCurrentClassLogger();

            var expectedOutput = "{\"TimeStamp\":\"" + TimeSource.Current.Time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ") + "\"," +
                                 "\"Level\":\"Trace\"," +
                                 "\"LoggerName\":\"" + LoggerName +
                                 "\",\"Message\":\"" + TestMessage + "\"" +
                                 ",\"One\":\"" + TestProperties.One + "\"" +
                                 ",\"Two\":\"" + TestProperties.Two + "\"" +
                                 ",\"Three\":\"" + TestProperties.Three + "\"}";

            var logEvent = new LogEventInfo(LogLevel.Trace, LoggerName, TestMessage);
            logger.Log(logEvent);

            target.Logs.Count.ShouldBe(1);

            var output = target.Logs[0];
            output.ShouldBe(expectedOutput);
        }

        public class GetFormattedMessage
        {
            [Test]
            public void AddsPropertyNamePrefixIfPropertyNameIsTheSameAsALogEventPropertyName()
            {
                const string existingPropertyName = "Level";

                TimeSource.Current = new FakeTimeSource();
                var logEvent = new LogEventInfo(LogLevel.Trace, LoggerName, TestMessage);

                var layout = new JsonWithPropertiesLayout();
                layout.Properties.Add(new StructuredLoggingProperty(existingPropertyName, new SimpleLayout(TestProperties.One)));

                var result = layout.Render(logEvent);

                var expectedPropertyName = JsonWithPropertiesLayout.PropertyNamePrefix + existingPropertyName;

                var expectedOutput = "{\"TimeStamp\":\"" + TimeSource.Current.Time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ") +
                                     "\",\"Level\":\"Trace" +
                                     "\",\"LoggerName\":\"" + LoggerName +
                                     "\",\"Message\":\"" + TestMessage +
                                     "\",\"" + expectedPropertyName + "\":\"" + TestProperties.One + "\"}";

                result.ShouldBe(expectedOutput, result);
            }
        }
    }
}

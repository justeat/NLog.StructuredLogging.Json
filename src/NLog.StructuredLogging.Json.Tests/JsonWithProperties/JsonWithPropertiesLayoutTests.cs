using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Time;
using NUnit.Framework;

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

            var expectedOutput =
                "{\"TimeStamp\":\"" + TimeSource.Current.Time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ") + "\"," +
                "\"Level\":\"Trace\"," +
                "\"LoggerName\":\"" + LoggerName +
                "\",\"Message\":\"" + TestMessage + "\"" +
                ",\"One\":\"" + TestProperties.One + "\"" +
                ",\"Two\":\"" + TestProperties.Two + "\"" +
                ",\"Three\":\"" + TestProperties.Three + "\"}";

            var logEvent = new LogEventInfo(LogLevel.Trace, LoggerName, TestMessage);
            logger.Log(logEvent);

            Assert.That(target.Logs.Count, Is.EqualTo(1));

            var output = target.Logs[0];
            output.ShouldBe(expectedOutput);
        }

        [Test]
        public void MachineNameInPropertyIsRendered()
        {
            const string targetName = "6ccc930b-111c-4df3-8b15-3d5668ea6f00";

            var layout = new JsonWithPropertiesLayout();
            layout.Properties.Add(new StructuredLoggingProperty("machinename", "${machinename}"));

            var target = new MemoryTarget
            {
                Name = targetName,
                Layout = layout
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            TimeSource.Current = new FakeTimeSource();
            var logger = LogManager.GetCurrentClassLogger();

            var logEvent = new LogEventInfo(LogLevel.Trace, LoggerName, TestMessage);
            logger.Log(logEvent);

            Assert.That(target.Logs.Count, Is.EqualTo(1));

            var output = target.Logs[0];
            Assert.That(output, Does.Contain("\"machinename\":\""));
            Assert.That(output, Does.Not.Contain("${machinename}"));
        }

        [Test, Ignore("Not working")]
        public void VarInPropertyIsRendered()
        {
            const string targetName = "18831108-76ba-417a-a92a-eb1bbf0738de";

            var layout = new JsonWithPropertiesLayout();
            layout.Properties.Add(new StructuredLoggingProperty("foo", "${var:foo}"));

            var target = new MemoryTarget
            {
                Name = targetName,
                Layout = layout
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            LogManager.Configuration.Variables.Add("foo", "fooVarValue");

            TimeSource.Current = new FakeTimeSource();
            var logger = LogManager.GetCurrentClassLogger();

            var logEvent = new LogEventInfo(LogLevel.Trace, LoggerName, TestMessage);
            logger.Log(logEvent);

            Assert.That(target.Logs.Count, Is.EqualTo(1));

            var output = target.Logs[0];
            Assert.That(output, Does.Contain("\"foo\":\""));
            Assert.That(output, Does.Contain("fooVarValue"));
            Assert.That(output, Does.Not.Contain("${var:foo}"));
        }


        [Test]
        public void PropertyRenderFailure()
        {
            const string targetName = "0255b1aa-8d55-4163-878c-e003431a4796";

            var layout = new JsonWithPropertiesLayout();
            layout.Properties.Add(new StructuredLoggingProperty("One", new FailingLayout()));
            layout.Properties.Add(new StructuredLoggingProperty("Two", new SimpleLayout(TestProperties.Two.ToString())));

            var target = new MemoryTarget
            {
                Name = targetName,
                Layout = layout
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            TimeSource.Current = new FakeTimeSource();
            var logger = LogManager.GetCurrentClassLogger();

            var expectedOutput =
                "{\"TimeStamp\":\"" + TimeSource.Current.Time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ") + "\"," +
                "\"Level\":\"Trace\"," +
                "\"LoggerName\":\"" + LoggerName +
                "\",\"Message\":\"" + TestMessage + "\"" +
                ",\"One\":\"Render failed: LoggingException Test render fail\"" +
                ",\"Two\":\"" + TestProperties.Two + "\"}";

            var logEvent = new LogEventInfo(LogLevel.Trace, LoggerName, TestMessage);
            logger.Log(logEvent);

            Assert.That(target.Logs.Count, Is.EqualTo(1));

            var output = target.Logs[0];
            output.ShouldBe(expectedOutput);
        }

        [Test]
        public void WhenPropertyNamesAreDuplicated()
        {
            const string targetName = "650b6a6b-b913-4b36-b594-e9073baf66da";

            var layout = new JsonWithPropertiesLayout();
            layout.Properties.Add(new StructuredLoggingProperty("duplicate", new SimpleLayout("value1")));
            layout.Properties.Add(new StructuredLoggingProperty("duplicate", new SimpleLayout("value2")));
            layout.Properties.Add(new StructuredLoggingProperty("duplicate", new SimpleLayout("value3")));

            var target = new MemoryTarget
            {
                Name = targetName,
                Layout = layout
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            TimeSource.Current = new FakeTimeSource();
            var logger = LogManager.GetCurrentClassLogger();

            var expectedOutput =
                "{\"TimeStamp\":\"" + TimeSource.Current.Time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ") + "\"," +
                "\"Level\":\"Trace\"," +
                "\"LoggerName\":\"" + LoggerName +
                "\",\"Message\":\"" + TestMessage + "\"" +
                ",\"duplicate\":\"value1\"" +
                ",\"properties_duplicate\":\"value2\"}";

            var logEvent = new LogEventInfo(LogLevel.Trace, LoggerName, TestMessage);
            logger.Log(logEvent);

            Assert.That(target.Logs.Count, Is.EqualTo(1));

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

                var expectedOutput =
                    "{\"TimeStamp\":\"" + TimeSource.Current.Time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ") +
                    "\",\"Level\":\"Trace" +
                    "\",\"LoggerName\":\"" + LoggerName +
                    "\",\"Message\":\"" + TestMessage +
                    "\",\"" + expectedPropertyName + "\":\"" + TestProperties.One + "\"}";

                Assert.That(result, Is.EqualTo(expectedOutput));
            }
        }
    }
}

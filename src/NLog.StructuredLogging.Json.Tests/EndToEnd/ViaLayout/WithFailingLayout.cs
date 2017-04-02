﻿using System.Linq;
using NLog.Config;
using NLog.Layouts;
using NLog.StructuredLogging.Json.Tests.JsonWithProperties;
using NLog.Targets;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class WithFailingLayout
    {
        [Test]
        public void WhenLayoutFails()
        {
            // arrange
            const string loggerName = "failingLogger";
            GivenLoggingIsConfiguredForTest(GivenFailingTarget(loggerName));
            var logger = LogManager.GetLogger(loggerName);

            // act
            logger.ExtendedInfo("test message", new { prop1 = "value1", prop2 = 2 });

            LogManager.Flush();

            var output = LogManager.Configuration.LogMessage(loggerName).First();

            Assert.That(output, Does.StartWith(
                "{\"fail1\":\"Render failed: LoggingException Test render fail\",\"flat1\":\"flat1\",\"TimeStamp\":\""));
            Assert.That(output, Does.EndWith(
                "\"prop1\":\"value1\",\"prop2\":\"2\"}"));
        }

        [Test]
        public void WhenPropertyNamesAreDuplicated()
        {
            // arrange
            const string loggerName = "duplicatingLogger";
            GivenLoggingIsConfiguredForTest(GivenTargetWithDuplicates(loggerName));
            var logger = LogManager.GetLogger(loggerName);

            // act
            logger.ExtendedInfo("test message", new { prop1 = "value1", prop2 = 2 });

            LogManager.Flush();

            var output = LogManager.Configuration.LogMessage(loggerName).First();

            Assert.That(output, Does.StartWith(
                "{\"duplicated\":\"value1\",\"attributes_duplicated\":\"value2\",\"TimeStamp\":\""));
            Assert.That(output, Does.EndWith(
                "\"prop1\":\"value1\",\"prop2\":\"2\"}"));
        }

        private void GivenLoggingIsConfiguredForTest(Target target)
        {
            ConfigurationItemFactory.Default.Layouts.RegisterDefinition("jsonwithproperties", typeof (JsonWithPropertiesLayout));
            ConfigurationItemFactory.Default.Layouts.RegisterDefinition("flattenedjsonlayout", typeof (FlattenedJsonLayout));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("structuredlogging.json", typeof(StructuredLoggingLayoutRenderer));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("hasher", typeof(HasherLayoutRenderer));

            var config = LogManager.Configuration;
            config.AddTarget(target);
            var rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Insert(0, rule);
            LogManager.Configuration = config;
        }

        private Target GivenFailingTarget(string name)
        {
            var layout = new FlattenedJsonLayout();
            layout.Attributes.Add(new JsonAttribute("fail1", new FailingLayout()));
            layout.Attributes.Add(new JsonAttribute("flat1", "flat1"));
            return new MemoryTarget
            {
                Name = name,
                Layout = layout
            };
        }

        private Target GivenTargetWithDuplicates(string name)
        {
            var layout = new FlattenedJsonLayout();
            layout.Attributes.Add(new JsonAttribute("duplicated", "value1"));
            layout.Attributes.Add(new JsonAttribute("duplicated", "value2"));
            layout.Attributes.Add(new JsonAttribute("duplicated", "value3"));

            return new MemoryTarget
            {
                Name = name,
                Layout = layout
            };
        }
    }
}

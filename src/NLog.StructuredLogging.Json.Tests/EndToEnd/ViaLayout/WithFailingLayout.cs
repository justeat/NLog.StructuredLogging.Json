using System.Linq;
using NLog.Config;
using NLog.Layouts;
using NLog.StructuredLogging.Json.Tests.JsonWithProperties;
using NLog.Targets;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class WithFailingLayout
    {
        private Logger _sut;

        [Test]
        public void WhenLayoutFails()
        {
            // arrange
            GivenLoggingIsConfiguredForTest("sut");
            _sut = LogManager.GetLogger("sut");

            // act
            _sut.ExtendedInfo("test message", new { prop1 = "value1", prop2 = 2 });

            LogManager.Flush();

            var output = LogManager.Configuration.LogMessage("sut").First();

            Assert.That(output, Does.StartWith(
                "{\"fail1\":\"Render failed: ApplicationException Test render fail\",\"flat1\":\"flat1\",\"TimeStamp\":\""));
            Assert.That(output, Does.EndWith(
                "\"prop1\":\"value1\",\"prop2\":\"2\"}"));
        }


        private void GivenLoggingIsConfiguredForTest(string name)
        {
            ConfigurationItemFactory.Default.Layouts.RegisterDefinition("jsonwithproperties", typeof (JsonWithPropertiesLayout));
            ConfigurationItemFactory.Default.Layouts.RegisterDefinition("flattenedjsonlayout", typeof (FlattenedJsonLayout));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("structuredlogging.json", typeof(StructuredLoggingLayoutRenderer));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("hasher", typeof(HasherLayoutRenderer));
            var config = LogManager.Configuration;
            var target = GivenTarget(name);
            config.AddTarget(target);
            var rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Insert(0, rule);
            LogManager.Configuration = config;
        }

        private Target GivenTarget(string name)
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
    }
}

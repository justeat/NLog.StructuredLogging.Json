using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;
using Shouldly;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class SimpleJsonInMessage
    {
        private static readonly string _name = "sut";
        private string _message;
        private IList<string> _output;
        private Logger _sut;
        private int _iterations;

        [TestFixtureSetUp]
        public void BeforeEverything()
        {
            Given();
            _sut = CreateSystemUnderTest();
            When();
        }

        private void When()
        {
            for (var i = 1; i <= _iterations; i++)
            {
                _sut.ExtendedInfo(_message, new {prop1 = "value1", prop2 = 2, iteration = i});
            }

            LogManager.Flush();
            _output = LogManager.Configuration.LogMessage(_name);
        }

        private static Logger CreateSystemUnderTest()
        {
            return LogManager.GetLogger(_name);
        }

        private void Given()
        {
            _iterations = 10;
            _message = string.Format("json start {0} json end",
                JsonConvert.SerializeObject(new {foo = "bar", sub = new {sub1 = "value"}}));
            GivenLoggingIsConfiguredForTest(_name);
        }

        private void GivenLoggingIsConfiguredForTest(string name)
        {
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
            layout.Attributes.Add(new JsonAttribute("flat1", "flat1"));
            return new MemoryTarget
            {
                Name = name,
                Layout = layout
            };
        }

        [Test]
        public void ShouldWriteValidJson()
        {
            foreach (var line in _output)
            {
                line.Count(x => x == '{').ShouldBe(3, line);
            }
        }
    }
}

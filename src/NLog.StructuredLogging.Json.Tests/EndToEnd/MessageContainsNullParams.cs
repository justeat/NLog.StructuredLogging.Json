using Newtonsoft.Json.Linq;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    [TestFixture]
    public class MessageContainsNullParams
    {
        private string _parameter1;
        private string _parameter2;
        private MemoryTarget _target;


        [SetUp]
        public void SetupTests()
        {
            ConfigurationItemFactory.Default.Layouts.RegisterDefinition("flattenedjsonlayout", typeof(FlattenedJsonLayout));

            _parameter1 = "With Extra String!!";
            _parameter2 = null;

            _target = new MemoryTarget();
            _target.Layout = new FlattenedJsonLayout();
        }

        [TearDown]
        public void TearDown()
        {
            LogManager.Configuration = new LoggingConfiguration();
        }

        [Test]
        public void InfoMessageWithStringFormatWillLogNullParameters()
        {
            SimpleConfigurator.ConfigureForTargetLogging(_target, LogLevel.Info);
            Logger logger = LogManager.GetLogger("Example");
            logger.Info("log message {0} and with a {1}", _parameter1, _parameter2);

            JToken log = JObject.Parse(_target.Logs[0]);
            string message = (string)log.SelectToken("Message");
            string parameter = (string) log.SelectToken("Parameters");

            Assert.That(message, Is.EqualTo("log message With Extra String!! and with a "));
            Assert.That(parameter, Is.EqualTo("With Extra String!!,null"));
        }

        [Test]
        public void WarnMessageWithStringFormatWillLogNullParameters()
        {
            SimpleConfigurator.ConfigureForTargetLogging(_target, LogLevel.Warn);
            Logger logger = LogManager.GetLogger("Example");
            logger.Warn("log message {0} and with a {1}", _parameter1, _parameter2);

            JToken log = JObject.Parse(_target.Logs[0]);
            string message = (string)log.SelectToken("Message");
            string parameter = (string)log.SelectToken("Parameters");

            Assert.That(message, Is.EqualTo("log message With Extra String!! and with a "));
            Assert.That(parameter, Is.EqualTo("With Extra String!!,null"));
        }

        [Test]
        public void ErrorMessageWithStringFormatWillLogNullParameters()
        {
            SimpleConfigurator.ConfigureForTargetLogging(_target, LogLevel.Error);
            Logger logger = LogManager.GetLogger("Example");
            logger.Error("log message {0} and with a {1}", _parameter1, _parameter2);

            JToken log = JObject.Parse(_target.Logs[0]);
            string message = (string)log.SelectToken("Message");
            string parameter = (string)log.SelectToken("Parameters");

            Assert.That(message, Is.EqualTo("log message With Extra String!! and with a "));
            Assert.That(parameter, Is.EqualTo("With Extra String!!,null"));
        }
    }
}
using NLog.Config;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class ConfigFileTests
    {
        [Test]
        public void ThrowExceptionsFlagShouldBeRead()
        {
            // throwExceptions="true" should have been read from nlog.config
            Assert.That(LogManager.ThrowExceptions, Is.True);
        }

        [Test]
        public void ConfigurationTargetsIsPopulated()
        {
            var config = LoadConfig();

            Assert.That(config.AllTargets, Is.Not.Null);
            Assert.That(config.AllTargets, Is.Not.Empty);
        }

        [Test]
        public void ConfigurationRulesIsPopulated()
        {
            var config = LoadConfig();

            Assert.That(config.LoggingRules, Is.Not.Null);
            Assert.That(config.LoggingRules, Is.Not.Empty);
        }

        private static LoggingConfiguration LoadConfig()
        {
            return new XmlLoggingConfiguration("nlog.config");
        }
    }
}
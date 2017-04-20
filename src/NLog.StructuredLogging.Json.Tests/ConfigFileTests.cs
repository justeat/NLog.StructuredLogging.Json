using System;
using System.IO;
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
            var result = new XmlLoggingConfiguration(GetConfigPath());
            Assert.That(result, Is.Not.Null);
            return result;
        }

        private static string GetConfigPath()
        {
            var dir = AppContext.BaseDirectory;
            var configPath = Path.Combine(dir, "nlog.config");
            Assert.That(File.Exists(configPath), $"Can't find config file at path '{configPath}'");
            return configPath;
        }
    }
}
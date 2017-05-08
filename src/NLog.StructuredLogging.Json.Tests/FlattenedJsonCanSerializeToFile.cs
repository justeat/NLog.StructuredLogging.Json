using System;
using System.IO;
using Newtonsoft.Json.Linq;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class FlattenedJsonCanSerializeToFile
    {
        [Test]
        public void JsonOutputIsCorrect()
        {
            var fileName = Path.GetTempFileName();

            var fileLogger = GetFlattenedJsonFileLogger(fileName);
            Assert.That(fileLogger, Is.Not.Null);

            fileLogger.ExtendedInfo("This is a test", 
                new
                {
                    Hello = "Hello",
                    SomeText = "this is some, text. It has! punctation?",
                    ANumber = 42,
                    Sometime = new DateTime(2017, 4, 5, 6, 7, 8, DateTimeKind.Utc)                    
                });

            LogManager.Flush();
            AssertIsJsonLogEntry(fileName);
            ClearLoggingConfig();
        }

        private void AssertIsJsonLogEntry(string fileName)
        {
            var json = ParseFile(fileName);

            // it should be json
            Assert.That(json, Is.Not.Null);
            Assert.That(json.HasValues);

            // standard props should be present
            Assert.That(json.GetValue("Message"), Is.Not.Null);
            Assert.That(json.GetValue("TimeStamp"), Is.Not.Null);
            Assert.That(json.GetValue("Level"), Is.Not.Null);

            // custom props should be present
            Assert.That(json.GetValue("Hello"), Is.Not.Null);
            Assert.That(json.GetValue("SomeText"), Is.Not.Null);
            Assert.That(json.GetValue("ANumber"), Is.Not.Null);
            Assert.That(json.GetValue("Sometime"), Is.Not.Null);

            // but not true for any old string
            Assert.That(json.GetValue("noSuchProp12345asdfg"), Is.Null);
        }

        private static JObject ParseFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Assert.Fail($"no output file exists with name {fileName}");
            }

            var contents = File.ReadAllText(fileName);

            if (string.IsNullOrWhiteSpace(contents))
            {
                Assert.Fail($"Output file {fileName} has no  contents");
            }

            // this will throw if the json is not valid
            return JObject.Parse(contents);
        }

        private Logger GetFlattenedJsonFileLogger(string fileName)
        {
            var config = new LoggingConfiguration();

            var target = new FileTarget
            {
                Name = $"test to file {fileName}",
                FileName = fileName,
                Layout = new FlattenedJsonLayout()
            };

            config.AddTarget(target);

            var rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
            return LogManager.GetCurrentClassLogger();
        }

        private void ClearLoggingConfig()
        {
            LogManager.Configuration = new LoggingConfiguration();
        }
    }
}
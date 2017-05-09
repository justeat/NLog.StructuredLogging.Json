using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using FakeItEasy;
using FakeItEasy.Configuration;

namespace NLog.StructuredLogging.Json.Tests
{
    [Category("mdlc")]
    [TestFixture]
    public class LoggerExtensionsContextTests
    {
        private ILogger _logger;
        public ArgumentCollection Arguments { get; set; }

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger>();
            A.CallTo(() => _logger.Name).Returns("FakeLogger");
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).Invokes(x => Arguments = x.Arguments);
        }

        [Test]
        public void WhenValuesAreSetInMDLContext_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("a2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("a3", 34);

            var props = new { Key1 = "Value One" };
            _logger.ExtendedInfo("hello world", props);

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Info, parameters.Level);
            Assert.IsNotEmpty(parameters.Properties);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", parameters.Properties["Key1"]);

            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("a2")));
            Assert.AreEqual("Value Two", parameters.Properties["a2"]);

            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("a3")));
            Assert.AreEqual("34", parameters.Properties["a3"]);

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void WhenValuesAreSetInMDLContextInCaller_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("b2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("b3", 34);

            MethodThatDoesSomeLogging();

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Info, parameters.Level);
            Assert.IsNotEmpty(parameters.Properties);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", parameters.Properties["Key1"]);

            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("b2")));
            Assert.AreEqual("Value Two", parameters.Properties["b2"]);

            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("b3")));
            Assert.AreEqual("34", parameters.Properties["b3"]);

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task WhenValuesAreSetInMDLContextInAsyncCaller_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("c2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("c3", 34);

            await MethodThatDoesSomeLoggingAsync();

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Info, parameters.Level);
            Assert.IsNotEmpty(parameters.Properties);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", parameters.Properties["Key1"]);

            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("c2")));
            Assert.AreEqual("Value Two", parameters.Properties["c2"]);

            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("c3")));
            Assert.AreEqual("34", parameters.Properties["c3"]);

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void WhenValuesAreSetInMDLContext_Clash()
        {
            MappedDiagnosticsLogicalContext.Set("Key1", "Value MDLC");

            var props = new { Key1 = "Value One" };
            _logger.ExtendedInfo("hello world", props);

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Info, parameters.Level);
            Assert.IsNotEmpty(parameters.Properties);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", parameters.Properties["Key1"]);

            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("log_context_Key1")));
            Assert.AreEqual("Value MDLC", parameters.Properties["log_context_Key1"]);

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        private void MethodThatDoesSomeLogging()
        {
            var props = new { Key1 = "Value One"};
            _logger.ExtendedInfo("hello world", props);
        }

        private async Task MethodThatDoesSomeLoggingAsync()
        {
            await Task.Delay(100).ConfigureAwait(false);
            var props = new { Key1 = "Value One" };
            _logger.ExtendedInfo("hello world", props);
        }
    }
}

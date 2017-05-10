using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using FakeItEasy;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class LoggerExtensionsContextTests
    {
        private ILogger _logger;
        private ConcurrentBag<LogEventInfo> _events;

        [SetUp]
        public void SetUp()
        {
            _events = new ConcurrentBag<LogEventInfo>();

            _logger = A.Fake<ILogger>();
            A.CallTo(() => _logger.Name).Returns("FakeLogger");
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored))
                .Invokes(x => _events.Add((LogEventInfo)x.Arguments[0]));
        }

        [Test]
        public void WhenValuesAreSetInMDLContext_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("a2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("a3", 34);

            var props = new { Key1 = "Value One" };
            _logger.ExtendedInfo("hello world", props);

            var eventInfo = _events.First();
            Assert.AreEqual(LogLevel.Info, eventInfo.Level);
            Assert.IsNotEmpty(eventInfo.Properties);
            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", eventInfo.Properties["Key1"]);

            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("a2")));
            Assert.AreEqual("Value Two", eventInfo.Properties["a2"]);

            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("a3")));
            Assert.AreEqual("34", eventInfo.Properties["a3"]);

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void WhenValuesAreSetInMDLContextInCaller_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("b2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("b3", 34);

            MethodThatDoesSomeLogging();

            var eventInfo = _events.First();
            Assert.AreEqual(LogLevel.Info, eventInfo.Level);
            Assert.IsNotEmpty(eventInfo.Properties);
            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", eventInfo.Properties["Key1"]);

            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("b2")));
            Assert.AreEqual("Value Two", eventInfo.Properties["b2"]);

            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("b3")));
            Assert.AreEqual("34", eventInfo.Properties["b3"]);

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task WhenValuesAreSetInMDLContextInAsyncCaller_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("c2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("c3", 34);

            await MethodThatDoesSomeLoggingAsync();

            var eventInfo = _events.First();
            Assert.AreEqual(LogLevel.Info, eventInfo.Level);
            Assert.IsNotEmpty(eventInfo.Properties);
            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", eventInfo.Properties["Key1"]);

            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("c2")));
            Assert.AreEqual("Value Two", eventInfo.Properties["c2"]);

            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("c3")));
            Assert.AreEqual("34", eventInfo.Properties["c3"]);

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void WhenValuesAreSetInMDLContext_Clash()
        {
            MappedDiagnosticsLogicalContext.Set("Key1", "Value MDLC");

            var props = new { Key1 = "Value One" };
            _logger.ExtendedInfo("hello world", props);

            var eventInfo = _events.First();
            Assert.AreEqual(LogLevel.Info, eventInfo.Level);
            Assert.IsNotEmpty(eventInfo.Properties);
            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", eventInfo.Properties["Key1"]);

            Assert.AreEqual(1, eventInfo.Properties.Count(x => x.Key.Equals("log_context_Key1")));
            Assert.AreEqual("Value MDLC", eventInfo.Properties["log_context_Key1"]);

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task WhenLoggingHappensInParallel()
        {
            const int taskCount = 12;
            MappedDiagnosticsLogicalContext.Set("parallelContext", "From MDLC");

            var tasks = new List<Task>();
            for (var i = 0; i < taskCount; i++)
            {
               tasks.Add(LogSomethingAsync(i));
            }

            await Task.WhenAll(tasks);

            Assert.That(_events.Count, Is.EqualTo(taskCount));

            foreach (var logEventInfo in _events)
            {
                Assert.AreEqual(LogLevel.Info, logEventInfo.Level);
                Assert.That(logEventInfo.Message, Does.StartWith("Info in task"));
                Assert.IsNotEmpty(logEventInfo.Properties);
                Assert.AreEqual(1, logEventInfo.Properties.Count(x => x.Key.Equals("parallelContext")));
                Assert.AreEqual("From MDLC", logEventInfo.Properties["parallelContext"]);
            }
        }

        private async Task LogSomethingAsync(int iter)
        {
            await Task.Delay(10);
            var props = new { Key1 = $"Value {iter}" };
            _logger.ExtendedInfo($"Info in task {iter}", props);
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

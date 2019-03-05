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
            MappedDiagnosticsLogicalContext.Clear();
            _events = new ConcurrentBag<LogEventInfo>();

            _logger = A.Fake<ILogger>();
            A.CallTo(() => _logger.Name).Returns("FakeLogger");
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored))
                .Invokes(x => _events.Add((LogEventInfo)x.Arguments[0]));
        }

        [TearDown]
        public void Teardown()
        {
            MappedDiagnosticsLogicalContext.Clear();
        }

        [Test]
        public void WhenValuesAreSetInMDLContext_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("a2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("a3", 34);

            var props = new { Key1 = "Value One" };
            _logger.ExtendedInfo("hello world", props);

            var eventInfo = _events.Single();
            Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Info));
            Assert.That(eventInfo.Properties, Is.Not.Empty);
            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));

            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("a2")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["a2"], Is.EqualTo("Value Two"));

            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("a3")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["a3"], Is.EqualTo("34"));

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(1, Times.Exactly);
        }

        [Test]
        public void WhenValuesAreSetInMDLContextInCaller_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("b2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("b3", 34);

            MethodThatDoesSomeLogging();

            var eventInfo = _events.Single();
            Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Info));
            Assert.That(eventInfo.Properties, Is.Not.Empty);
            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));

            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("b2")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["b2"], Is.EqualTo("Value Two"));

            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("b3")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["b3"], Is.EqualTo("34"));

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(1, Times.Exactly);
        }

        [Test]
        public async Task WhenValuesAreSetInMDLContextInAsyncCaller_TheyAreHarvestedToDictionary()
        {
            MappedDiagnosticsLogicalContext.Set("c2", "Value Two");
            MappedDiagnosticsLogicalContext.Set("c3", 34);

            await MethodThatDoesSomeLoggingAsync();

            var eventInfo = _events.Single();
            Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Info));
            Assert.That(eventInfo.Properties, Is.Not.Empty);
            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));

            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("c2")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["c2"], Is.EqualTo("Value Two"));

            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("c3")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["c3"], Is.EqualTo("34"));

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(1, Times.Exactly);
        }

        [Test]
        public void WhenValuesAreSetInMDLContext_Clash()
        {
            MappedDiagnosticsLogicalContext.Set("Key1", "Value MDLC");

            var props = new { Key1 = "Value One" };
            _logger.ExtendedInfo("hello world", props);

            var eventInfo = _events.Single();
            Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Info));
            Assert.That(eventInfo.Properties, Is.Not.Empty);
            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));

            Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("log_context_Key1")), Is.EqualTo(1));
            Assert.That(eventInfo.Properties["log_context_Key1"], Is.EqualTo("Value MDLC"));

            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(1, Times.Exactly);
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
                Assert.That(logEventInfo.Level, Is.EqualTo(LogLevel.Info));
                Assert.That(logEventInfo.Message, Does.StartWith("Info in task"));
                Assert.That(logEventInfo.Properties, Is.Not.Empty);
                Assert.That(logEventInfo.Properties.Count(x => x.Key.Equals("parallelContext")), Is.EqualTo(1));
                Assert.That(logEventInfo.Properties["parallelContext"], Is.EqualTo("From MDLC"));
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
            var props = new { Key1 = "Value One" };
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

using NUnit.Framework;
using FakeItEasy;
using System;
using System.Collections.Generic;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class NestedExceptionTests
    {
        private ILogger _logger;

        [SetUp]
        public void SetUp()
        {
            MappedDiagnosticsLogicalContext.Clear();
            _logger = A.Fake<ILogger>();
            A.CallTo(() => _logger.Name).Returns("FakeLogger");
        }

        private void CaptureLogInfoToList(IList<LogEventInfo> list)
        {
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored))
                .Invokes(x => list.Add((LogEventInfo)x.Arguments[0]));
        }

        [Test]
        public void SimpleException_IsLogged()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var ex = new Exception("example exception");

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(1));
            Assert.That(itemsLogged[0].Message, Is.EqualTo("Test message"));
            Assert.That(itemsLogged[0].Exception, Is.EqualTo(ex));
        }

        [Test]
        public void SimpleException_HasExpectedProperties()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var ex = new Exception("example exception");

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(1));
            Assert.That(itemsLogged[0].Properties.Count, Is.EqualTo(3));
            AssertAllHaveStandardKeyValue(itemsLogged);
            AssertHasExceptionContext(itemsLogged[0], 1, 1);
        }

        [Test]
        public void InnerException()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var ex = new Exception("example exception", new Exception("inner"));

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(2));
            Assert.That(itemsLogged[0].Exception, Is.EqualTo(ex));
            Assert.That(itemsLogged[1].Exception, Is.EqualTo(ex.InnerException));

            AssertAllHaveStandardKeyValue(itemsLogged);
            AssertHasExceptionContext(itemsLogged[0], 1, 2);
            AssertHasExceptionContext(itemsLogged[1], 2, 2);
        }

        [Test]
        public void InnerExceptionTracking()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var ex = new Exception("example exception", new Exception("inner"));

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(2));

            Assert.That(itemsLogged[0].Properties.Count, Is.EqualTo(4));
            Assert.That(itemsLogged[1].Properties.Count, Is.EqualTo(4));

            AssertAllHaveStandardKeyValue(itemsLogged);

            AssertHasExceptionContext(itemsLogged[0], 1, 2);
            AssertHasExceptionContext(itemsLogged[1], 2, 2);
        }

        [Test]
        public void InnerInnerException()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var ex = new Exception("example exception", new Exception("inner", new Exception("Inner inner")));

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(3));
            Assert.That(itemsLogged[0].Exception, Is.EqualTo(ex));
            Assert.That(itemsLogged[1].Exception, Is.EqualTo(ex.InnerException));
            Assert.That(itemsLogged[2].Exception, Is.EqualTo(ex.InnerException.InnerException));

            AssertAllHaveStandardKeyValue(itemsLogged);
            AssertHasExceptionContext(itemsLogged[0], 1, 3);
            AssertHasExceptionContext(itemsLogged[1], 2, 3);
            AssertHasExceptionContext(itemsLogged[2], 3, 3);
        }

        [Test]
        public void AggregateExceptionWithOneContained()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var ex = new AggregateException("example exception", new Exception("inner 1"));

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(2));
            Assert.That(itemsLogged[0].Exception, Is.EqualTo(ex));
            Assert.That(itemsLogged[1].Exception, Is.EqualTo(ex.InnerExceptions[0]));

            AssertAllHaveStandardKeyValue(itemsLogged);
            AssertHasExceptionContext(itemsLogged[0], 1, 2);
            AssertHasExceptionContext(itemsLogged[1], 2, 2);
        }

        [Test]
        public void AggregateExceptionWithTwoContained()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var ex = new AggregateException("example exception", new Exception("inner 1"), new Exception("inner 2"));

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(3));
            Assert.That(itemsLogged[0].Exception, Is.EqualTo(ex));
            Assert.That(itemsLogged[1].Exception, Is.EqualTo(ex.InnerExceptions[0]));
            Assert.That(itemsLogged[2].Exception, Is.EqualTo(ex.InnerExceptions[1]));

            AssertAllHaveStandardKeyValue(itemsLogged);
            AssertHasExceptionContext(itemsLogged[0], 1, 3);
            AssertHasExceptionContext(itemsLogged[1], 2, 3);
            AssertHasExceptionContext(itemsLogged[2], 3, 3);
        }

        [Test]
        public void AggregateAndInnerException()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var exWithInner = new Exception("example exception", new Exception("inner"));
            var ex = new AggregateException("example exception", new Exception("inner 1"), exWithInner);

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(4));

            Assert.That(itemsLogged[0].Exception, Is.EqualTo(ex));
            Assert.That(itemsLogged[1].Exception, Is.EqualTo(ex.InnerExceptions[0]));
            Assert.That(itemsLogged[2].Exception, Is.EqualTo(ex.InnerExceptions[1]));
            Assert.That(itemsLogged[3].Exception, Is.EqualTo(ex.InnerExceptions[1].InnerException));

            AssertAllHaveStandardKeyValue(itemsLogged);
            AssertHasExceptionContext(itemsLogged[0], 1, 4);
            AssertHasExceptionContext(itemsLogged[1], 2, 4);
            AssertHasExceptionContext(itemsLogged[2], 3, 4);
            AssertHasExceptionContext(itemsLogged[3], 4, 4);
        }

        [Test]
        public void DoubleAggregateException()
        {
            List<LogEventInfo> itemsLogged = new List<LogEventInfo>();
            CaptureLogInfoToList(itemsLogged);

            var innerAggregate = new AggregateException("inner aggregate", new Exception("inner ag 1"), new Exception("inner ag 2"));

            var ex = new AggregateException("example exception", new Exception("inner 1"), innerAggregate);

            _logger.ExtendedException(ex, "Test message", new { Key1 = "Value One" });

            Assert.That(itemsLogged.Count, Is.EqualTo(4));

            // the inner aggregate is removed by the call to "Flatten"
            Assert.That(itemsLogged[0].Exception, Is.EqualTo(ex));
            Assert.That(itemsLogged[1].Exception, Is.EqualTo(ex.InnerExceptions[0]));
            Assert.That(itemsLogged[2].Exception, Is.EqualTo(innerAggregate.InnerExceptions[0]));
            Assert.That(itemsLogged[3].Exception, Is.EqualTo(innerAggregate.InnerExceptions[1]));

            AssertAllHaveStandardKeyValue(itemsLogged);
            AssertHasExceptionContext(itemsLogged[0], 1, 4);
            AssertHasExceptionContext(itemsLogged[1], 2, 4);
            AssertHasExceptionContext(itemsLogged[2], 3, 4);
            AssertHasExceptionContext(itemsLogged[3], 4, 4);
        }

        private void AssertHasExceptionContext(LogEventInfo logItem, int index, int count)
        {
            Assert.That(logItem.Properties["ExceptionIndex"], Is.EqualTo(index), "Incorrect index");
            Assert.That(logItem.Properties["ExceptionCount"], Is.EqualTo(count), "Incorrect index");

            if (count == 1)
            {
                Assert.That(logItem.Properties.ContainsKey("ExceptionTag"), Is.False, "ExceptionTag was not expected");
            }
            else
            {
                Assert.That(logItem.Properties.ContainsKey("ExceptionTag"), Is.True, "ExceptionTag was expected");
                Assert.That(logItem.Properties["ExceptionTag"], Is.Not.Empty, "ExceptionTag was empty");
            }
        }

        private void AssertAllHaveStandardKeyValue(List<LogEventInfo> logItems)
        {
            foreach (var logItem in logItems)
            {
                AssertHasStandardKeyValue(logItem);
            }
        }

        private void AssertHasStandardKeyValue(LogEventInfo logItem)
        {
            Assert.That(logItem.Properties.ContainsKey("Key1"), Is.True, "Key1 was not present");
            Assert.That(logItem.Properties["Key1"], Is.EqualTo("Value One"), "Key1 did not have expected value");
        }
    }
}

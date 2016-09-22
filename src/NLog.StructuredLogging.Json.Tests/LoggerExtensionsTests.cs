using System;
using System.Linq;
using NUnit.Framework;
using FakeItEasy;
using FakeItEasy.Configuration;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class LoggerExtensionsTests
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
        public void ExtendedDebug_NoProperties_NoPropertiesSet()
        {
            _logger.ExtendedDebug("hello world", null);

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Debug, parameters.Level);
            Assert.IsEmpty(parameters.Properties);
        }

        [Test]
        public void ExtendedDebug_WithProperties_PublicPropertiesAreInjected()
        {
            _logger.ExtendedDebug("hello world", new { Key1 = "Value One", key2 = "Value Two" });

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Debug, parameters.Level);
            Assert.IsNotEmpty(parameters.Properties);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", parameters.Properties["Key1"]);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("key2")));
            Assert.AreEqual("Value Two", parameters.Properties["key2"]);
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ExtendedDebug_WithIndexerProperties_DoesNotThrow()
        {
            var badData = new PropertiesWithIndexer
            {
                Foo = "test",
                Bar = 42
            };
            _logger.ExtendedDebug("hello world", badData);
            Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        }

        [Test]
        public void ExtendedInfo_NoProperties_NoPropertiesSet()
        {
            _logger.ExtendedInfo("hello world", null);

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Info, parameters.Level);
            Assert.IsEmpty(parameters.Properties);
        }

        [Test]
        public void ExtendedInfo_WithProperties_PublicPropertiesAreInjected()
        {
            _logger.ExtendedInfo("hello world", new { Key1 = "Value One", key2 = "Value Two" });

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Info, parameters.Level);
            Assert.IsNotEmpty(parameters.Properties);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", parameters.Properties["Key1"]);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("key2")));
            Assert.AreEqual("Value Two", parameters.Properties["key2"]);
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ExtendedInfo_WithIndexerProperties_DoesNotThrow()
        {
            var badData = new PropertiesWithIndexer
            {
                Foo = "test",
                Bar = 42
            };
            _logger.ExtendedInfo("hello world", badData);
            Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        }

        [Test]
        public void ExtendedWarn_NoProperties_NoPropertiesSet()
        {
            _logger.ExtendedWarn("hello world", null);

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Warn, parameters.Level);
            Assert.IsEmpty(parameters.Properties);
        }

        [Test]
        public void ExtendedWarn_WithIndexerProperties_DoesNotThrow()
        {
            var badData = new PropertiesWithIndexer
            {
                Foo = "test",
                Bar = 42
            };
            _logger.ExtendedWarn("hello world", badData);
            Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        }

        [Test]
        public void ExtendedError_NoProperties_NoPropertiesSet()
        {
            _logger.ExtendedError("hello world", null);

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Error, parameters.Level);
            Assert.IsEmpty(parameters.Properties);
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ExtendedError_WithProperties_PublicPropertiesAreInjected()
        {
            _logger.ExtendedError("hello world", new { Key1 = "Value One", key2 = "Value Two" });

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Error, parameters.Level);
            Assert.IsNotEmpty(parameters.Properties);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", parameters.Properties["Key1"]);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("key2")));
            Assert.AreEqual("Value Two", parameters.Properties["key2"]);
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ExtendedError_WithIndexerProperties_DoesNotThrow()
        {
            var badData = new PropertiesWithIndexer
            {
                Foo = "test",
                Bar = 42
            };
            _logger.ExtendedError("hello world", badData);
            Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        }

        [Test]
        public void ExtendedException_NoProperties()
        {
            _logger.ExtendedException(new Exception("example exception"), "hello world", new {});

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Error, parameters.Level);
            Assert.NotNull(parameters.Exception);
            Assert.AreEqual("example exception", parameters.Exception.Message);
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ExtendedException_NullProperties()
        {
            _logger.ExtendedException(new Exception("example exception"), "hello world", null);

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Error, parameters.Level);
            Assert.NotNull(parameters.Exception);
            Assert.AreEqual("example exception", parameters.Exception.Message);
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ExtendedException_ImplicitNullProperties()
        {
            _logger.ExtendedException(new Exception("example exception"), "hello world");

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Error, parameters.Level);
            Assert.NotNull(parameters.Exception);
            Assert.AreEqual("example exception", parameters.Exception.Message);
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ExtendedException_NoProperties_OnlyExceptionTrackingPropertiesAreSet()
        {
            _logger.ExtendedException(new Exception("example exception"), "hello world", new { });

            var parameters = (LogEventInfo)Arguments[0];

            Assert.AreEqual(parameters.Properties.Count, 2);
            Assert.AreEqual(parameters.Properties["ExceptionIndex"], 1);
            Assert.AreEqual(parameters.Properties["ExceptionCount"], 1);
            Assert.IsFalse(parameters.Properties.ContainsKey("ExceptionTag"));
        }

        [Test]
        public void ExtendedException_WithProperties_PublicPropertiesAreInjected()
        {
            _logger.ExtendedException(new Exception("example exception"), "hello world", new { Key1 = "Value One", key2 = "Value Two" });

            var parameters = (LogEventInfo)Arguments[0];
            Assert.AreEqual(LogLevel.Error, parameters.Level);
            Assert.IsNotEmpty(parameters.Properties);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("Key1")));
            Assert.AreEqual("Value One", parameters.Properties["Key1"]);
            Assert.AreEqual(1, parameters.Properties.Count(x => x.Key.Equals("key2")));
            Assert.AreEqual("Value Two", parameters.Properties["key2"]);
            Assert.NotNull(parameters.Exception);
            Assert.AreEqual("example exception", parameters.Exception.Message);
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ExtendedExceptionWithStringOnlyDoesNotThrow()
        {
            _logger.ExtendedException(new Exception(), null, "boom!");
            Assert.Pass("This checks that the method overloads are able to cope with a string being passed as logProperties, since string >> object");
        }

        [Test]
        public void ExtendedException_WithIndexerProperties_DoesNotThrow()
        {
            var badData = new PropertiesWithIndexer
            {
                    Foo = "test",
                    Bar = 42
                };
            _logger.ExtendedException(new Exception(), "test", badData);
            Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        }
    }

    public class PropertiesWithIndexer
    {
        public string Foo { get; set; }
        public int Bar { get; set; }

        /// <summary>
        /// the indexer property, which requires an int param
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int this[int index]
        {
            get
            {
                return index * 2 + Bar;
            }
        }
    }
}

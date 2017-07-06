using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FakeItEasy;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class LoggerExtensionsScopeTests
    {
        private ILogger _logger;
        private ConcurrentBag<LogEventInfo> _events;

        private class ExpectedData
        {            
            public LogLevel LogLevel { get; set; }
            public Predicate<IDictionary<object, object>> PropertiesPredicate { get; set; }
            public string Message { get; set; }
        }

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
        public void ExtendedDebug_Out_Of_Scope_NoScopeProperties_NoPropertiesSet()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = "Start logical scope",
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&                                                        
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"])
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = "Finish logical scope",
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"])
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Debug,
                    Message = message,
                    PropertiesPredicate = properties => !properties.Any()
                }
            };

            using (_logger.BeginScope(scopeName).WithoutProperties())
            {
                
            }

            _logger.ExtendedDebug(message, null);

            var events = _events.Reverse().ToArray();
            for (int i = 0; i < events.Length; i++)
            {
                var eventInfo = events[i];
                var expected = expectedData[i];

                Assert.That(eventInfo.Level, Is.EqualTo(expected.LogLevel));
                Assert.That(expected.PropertiesPredicate(eventInfo.Properties));
                Assert.AreEqual(eventInfo.FormattedMessage, expected.Message);
            }                        
        }

        [Test]
        public void ExtendedDebug_Out_Of_Scope_ScopeProperties_NoPropertiesSet()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = "Start logical scope",
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&                                                        
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = "Finish logical scope",
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Debug,
                    Message = message,
                    PropertiesPredicate = properties => !properties.Any()
                }
            };

            using (_logger.BeginScope(scopeName, logProps).WithoutProperties())
            {

            }

            _logger.ExtendedDebug(message, null);

            var events = _events.Reverse().ToArray();
            for (int i = 0; i < events.Length; i++)
            {
                var eventInfo = events[i];
                var expected = expectedData[i];

                Assert.That(eventInfo.Level, Is.EqualTo(expected.LogLevel));
                Assert.That(expected.PropertiesPredicate(eventInfo.Properties));
                Assert.AreEqual(eventInfo.FormattedMessage, expected.Message);
            }
        }

        [Test]
        public void ExtendedDebug_ScopeProperties_NoPropertiesSet()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = "Start logical scope",
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Debug,
                    Message = message,
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"])                                                        
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = "Finish logical scope",
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                }
            };

            using (_logger.BeginScope(scopeName, logProps).WithoutProperties())
            {
                _logger.ExtendedDebug(message, null);
            }

            var events = _events.Reverse().ToArray();
            for (int i = 0; i < events.Length; i++)
            {
                var eventInfo = events[i];
                var expected = expectedData[i];

                Assert.That(eventInfo.Level, Is.EqualTo(expected.LogLevel));
                Assert.That(expected.PropertiesPredicate(eventInfo.Properties));
                Assert.AreEqual(eventInfo.FormattedMessage, expected.Message);
            }
        }

        [Test]
        public void ExtendedDebug_ScopeProperties_PropertiesSet()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = "Start logical scope",
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Debug,
                    Message = message,
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = "Finish logical scope",
                    PropertiesPredicate = properties => properties["Scope"].Equals(scopeName) &&
                                                        properties["ScopeTrace"].Equals(properties["ScopeId"]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                }
            };

            using (_logger.BeginScope(scopeName, logProps).WithProperties())
            {
                _logger.ExtendedDebug(message, null);
            }

            var events = _events.Reverse().ToArray();
            for (int i = 0; i < events.Length; i++)
            {
                var eventInfo = events[i];
                var expected = expectedData[i];

                Assert.That(eventInfo.Level, Is.EqualTo(expected.LogLevel));
                Assert.That(expected.PropertiesPredicate(eventInfo.Properties));
                Assert.AreEqual(eventInfo.FormattedMessage, expected.Message);
            }
        }

        //[Test]
        //public void ExtendedDebug_WithProperties_PublicPropertiesAreInjected()
        //{
        //    _logger.ExtendedDebug("hello world", new { Key1 = "Value One", key2 = "Value Two" });

        //    var eventInfo = _events.Single();
        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Debug));
        //    Assert.That(eventInfo.Properties, Is.Not.Empty);
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("key2")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["key2"], Is.EqualTo("Value Two"));
        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedDebug_WithIndexerProperties_DoesNotThrow()
        //{
        //    var badData = new PropertiesWithIndexer
        //    {
        //        Foo = "test",
        //        Bar = 42
        //    };
        //    _logger.ExtendedDebug("hello world", badData);
        //    Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        //}

        //[Test]
        //public void ExtendedInfo_NoProperties_NoPropertiesSet()
        //{
        //    _logger.ExtendedInfo("hello world", null);

        //    var eventInfo = _events.Single();
        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Info));
        //    Assert.That(eventInfo.Properties, Is.Empty);
        //}

        //[Test]
        //public void ExtendedInfo_WithProperties_PublicPropertiesAreInjected()
        //{
        //    _logger.ExtendedInfo("hello world", new { Key1 = "Value One", key2 = "Value Two" });

        //    var eventInfo = _events.Single();
        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Info));
        //    Assert.That(eventInfo.Properties, Is.Not.Empty);
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("key2")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["key2"], Is.EqualTo("Value Two"));
        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedInfo_WithDictionary_ValuesAreInjected()
        //{
        //    var props = new Dictionary<string, object>
        //        {
        //            { "Key1", "Value One" },
        //            { "key2", "Value Two" }
        //        };

        //    _logger.ExtendedInfo("hello world", props);

        //    var eventInfo = _events.Single();
        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Info));
        //    Assert.That(eventInfo.Properties, Is.Not.Empty);
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("key2")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["key2"], Is.EqualTo("Value Two"));
        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedInfo_WithIndexerProperties_DoesNotThrow()
        //{
        //    var badData = new PropertiesWithIndexer
        //    {
        //        Foo = "test",
        //        Bar = 42
        //    };
        //    _logger.ExtendedInfo("hello world", badData);
        //    Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        //}

        //[Test]
        //public void ExtendedWarn_NoProperties_NoPropertiesSet()
        //{
        //    _logger.ExtendedWarn("hello world", null);

        //    var eventInfo = _events.Single();
        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Warn));
        //    Assert.That(eventInfo.Properties, Is.Empty);
        //}

        //[Test]
        //public void ExtendedWarn_WithIndexerProperties_DoesNotThrow()
        //{
        //    var badData = new PropertiesWithIndexer
        //    {
        //        Foo = "test",
        //        Bar = 42
        //    };
        //    _logger.ExtendedWarn("hello world", badData);
        //    Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        //}

        //[Test]
        //public void ExtendedError_NoProperties_NoPropertiesSet()
        //{
        //    _logger.ExtendedError("hello world", null);

        //    var eventInfo = _events.Single();
        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Error));
        //    Assert.That(eventInfo.Properties, Is.Empty);
        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedError_WithProperties_PublicPropertiesAreInjected()
        //{
        //    _logger.ExtendedError("hello world", new { Key1 = "Value One", key2 = "Value Two" });

        //    var eventInfo = _events.Single();
        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Error));
        //    Assert.That(eventInfo.Properties, Is.Not.Empty);
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("key2")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["key2"], Is.EqualTo("Value Two"));
        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedError_WithIndexerProperties_DoesNotThrow()
        //{
        //    var badData = new PropertiesWithIndexer
        //    {
        //        Foo = "test",
        //        Bar = 42
        //    };
        //    _logger.ExtendedError("hello world", badData);
        //    Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        //}

        //[Test]
        //public void ExtendedException_NoProperties()
        //{
        //    _logger.ExtendedException(new Exception("example exception"), "hello world", new {});

        //    var eventInfo = _events.Single();

        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Error));

        //    Assert.That(eventInfo.Exception, Is.Not.Null);
        //    Assert.That(eventInfo.Exception.Message, Is.EqualTo("example exception"));

        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedException_NullProperties()
        //{
        //    _logger.ExtendedException(new Exception("example exception"), "hello world", null);

        //    var eventInfo = _events.Single();

        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Error));

        //    Assert.That(eventInfo.Exception, Is.Not.Null);
        //    Assert.That(eventInfo.Exception.Message, Is.EqualTo("example exception"));

        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedException_ImplicitNullProperties()
        //{
        //    _logger.ExtendedException(new Exception("example exception"), "hello world");

        //    var eventInfo = _events.Single();

        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Error));

        //    Assert.That(eventInfo.Exception, Is.Not.Null);
        //    Assert.That(eventInfo.Exception.Message, Is.EqualTo("example exception"));

        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedException_NoProperties_OnlyExceptionTrackingPropertiesAreSet()
        //{
        //    _logger.ExtendedException(new Exception("example exception"), "hello world", new { });

        //    var eventInfo = _events.Single();

        //    Assert.That(eventInfo.Properties.Count, Is.EqualTo(2));
        //    Assert.That(eventInfo.Properties["ExceptionIndex"], Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["ExceptionCount"], Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties.ContainsKey("ExceptionTag"), Is.False);
        //}

        //[Test]
        //public void ExtendedException_WithProperties_PublicPropertiesAreInjected()
        //{
        //    _logger.ExtendedException(new Exception("example exception"), "hello world", new { Key1 = "Value One", key2 = "Value Two" });

        //    var eventInfo = _events.Single();

        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Error));

        //    Assert.That(eventInfo.Properties, Is.Not.Empty);
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("key2")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["key2"], Is.EqualTo("Value Two"));

        //    Assert.That(eventInfo.Exception, Is.Not.Null);
        //    Assert.That(eventInfo.Exception.Message, Is.EqualTo("example exception"));
        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedException_WithDictionaryProperties()
        //{
        //    var logProperties = new Dictionary<object, object>
        //    {
        //        {"Key1", "Value One"},
        //        {"key2", 2}
        //    };

        //    _logger.ExtendedException(new Exception("example exception"), "hello world", logProperties);

        //    var eventInfo = _events.Single();

        //    Assert.That(eventInfo.Level, Is.EqualTo(LogLevel.Error));

        //    Assert.That(eventInfo.Properties, Is.Not.Empty);
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("Key1")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["Key1"], Is.EqualTo("Value One"));
        //    Assert.That(eventInfo.Properties.Count(x => x.Key.Equals("key2")), Is.EqualTo(1));
        //    Assert.That(eventInfo.Properties["key2"], Is.EqualTo(2));

        //    Assert.That(eventInfo.Exception, Is.Not.Null);

        //    Assert.That(eventInfo.Exception.Message, Is.EqualTo("example exception"));
        //    A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[Test]
        //public void ExtendedExceptionWithStringOnlyDoesNotThrow()
        //{
        //    _logger.ExtendedException(new Exception(), null, "boom!");
        //    Assert.Pass("This checks that the method overloads are able to cope with a string being passed as logProperties, since string >> object");
        //}

        //[Test]
        //public void ExtendedException_WithIndexerProperties_DoesNotThrow()
        //{
        //    var badData = new PropertiesWithIndexer
        //    {
        //            Foo = "test",
        //            Bar = 42
        //        };
        //    _logger.ExtendedException(new Exception(), "test", badData);
        //    Assert.Pass("This checks that the method overloads are able to cope with a object with an indexer property being passed to the logger");
        //}
    }    
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using FakeItEasy;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture("Debug")]
    [TestFixture("Info")]
    [TestFixture("Warn")]
    [TestFixture("Error")]
    public class LoggerExtensionsScopeTests
    {
        private const string StartLogicalScopeMessage = "Start logical scope";
        private const string ScopePropertyName = "Scope";
        private const string ScopeIdTracePropertyName = "ScopeIdTrace";
        private const string ScopeIdPropertyName = "ScopeId";
        private const string ScopeNameTracePropertyName = "ScopeNameTrace";
        private const string FinishLogicalScopeMessage = "Finish logical scope";
        private ILogger _logger;
        private ConcurrentQueue<LogEventInfo> _events;
        private readonly LogLevel _logLevel;

        private class ExpectedData
        {            
            public LogLevel LogLevel { get; set; }
            public Predicate<IDictionary<object, object>> PropertiesPredicate { get; set; }
            public string Message { get; set; }
        }        

        public LoggerExtensionsScopeTests(string logLevel)
        {            
            _logLevel = LogLevel.FromString(logLevel);
        }

        [SetUp]
        public void SetUp()
        {
            _events = new ConcurrentQueue<LogEventInfo>();
            _logger = A.Fake<ILogger>();

            A.CallTo(() => _logger.Name).Returns("FakeLogger");
            A.CallTo(() => _logger.Log(A<LogEventInfo>.Ignored))
                .Invokes(x => _events.Enqueue((LogEventInfo)x.Arguments[0]));
        }

        [TearDown]
        public void TearDown()
        {
            _events = null;
            NestedDiagnosticsLogicalContext.Clear();
        }

        [Test]
        public void When_Out_Of_Scope_NoScopeProperties_NoPropertiesSet()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&                                                        
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName])
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName])
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => !properties.Any()
                }
            };

            using (_logger.BeginScope(scopeName, configuration: new ScopeConfiguration{IncludeProperties = false}))
            {
                
            }

            _logger.Extended(_logLevel, message, null);

            var events = _events.ToArray();
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
        public void When_Out_Of_Scope_ScopeProperties_PropertiesSet()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&                                                        
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => !properties.Any()
                }
            };

            using (_logger.BeginScope(scopeName, logProps))
            {

            }

            _logger.Extended(_logLevel, message, null);

            var events = _events.ToArray();
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
        public void When_ScopeProperties_Default()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                }
            };

            using (_logger.BeginScope(scopeName, logProps))
            {
                _logger.Extended(_logLevel, message, null);
            }

            var events = _events.ToArray();
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
        public void When_ScopeNameTrace_Not_Included()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        !properties.ContainsKey(ScopeNameTracePropertyName) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        !properties.ContainsKey(ScopeNameTracePropertyName) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        !properties.ContainsKey(ScopeNameTracePropertyName) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                }
            };

            using (_logger.BeginScope(scopeName, logProps, new ScopeConfiguration{IncludeScopeNameTrace = false}))
            {
                _logger.Extended(_logLevel, message, null);
            }

            var events = _events.ToArray();
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
        public async Task When_Logging_Happens_In_Parallel()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };
            var taskOneLogProps = new { Task = "one"};
            var taskTwoLogProps = new { Task = "two" };

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2) &&
                                                        properties.ContainsKey("Task")
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2) &&
                                                        properties.ContainsKey("Task")
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                }
            };

            using (_logger.BeginScope(scopeName, logProps))
            {
                var taskOne = Task.Run(() => _logger.Extended(_logLevel, message, taskOneLogProps));
                var taskTwo = Task.Run(() => _logger.Extended(_logLevel, message, taskTwoLogProps));
                await Task.WhenAll(taskOne, taskTwo);
            }

            var events = _events.ToArray();
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
        public async Task When_Logging_Happens_In_Parallel_With_Nested_Scope()
        {
            const string scopeName = "empty scope";
            var nestedScopeForTaskTwo = "nested scope for task two";
            var nestedScopeForTaskOne = "nested scope for task one";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };
            var taskOneLogProps = new { Task = "one" };
            var taskTwoLogProps = new { Task = "two" };

            object topScopeId = null;

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScopeForTaskOne)&&
                                                        properties[ScopeIdTracePropertyName].Equals($"{topScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScopeForTaskTwo) &&
                                                        properties[ScopeIdTracePropertyName].Equals($"{topScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScopeForTaskOne) &&
                                                        properties[ScopeIdTracePropertyName].Equals($"{topScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2) &&
                                                        properties[nameof(taskOneLogProps.Task)].Equals(taskOneLogProps.Task)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScopeForTaskTwo) &&
                                                        properties[ScopeIdTracePropertyName].Equals($"{topScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2) &&
                                                        properties[nameof(taskTwoLogProps.Task)].Equals(taskTwoLogProps.Task)
                },                
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScopeForTaskOne)&&
                                                        properties[ScopeIdTracePropertyName].Equals($"{topScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScopeForTaskTwo) &&
                                                        properties[ScopeIdTracePropertyName].Equals($"{topScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                }
            };

            using (var topScope = _logger.BeginScope(scopeName, logProps))
            {
                topScopeId = topScope.ScopeId;
                var taskOne = Task.Run(() =>
                {                    
                    using (_logger.BeginScope(nestedScopeForTaskOne))
                    {
                        _logger.Extended(_logLevel, message, taskOneLogProps);
                    }
                });
                var taskTwo = Task.Run(() =>
                {                   
                    using (_logger.BeginScope(nestedScopeForTaskTwo))
                    {
                        _logger.Extended(_logLevel, message, taskTwoLogProps);
                    }
                });
                await Task.WhenAll(taskOne, taskTwo);
            }

            var events = _events.ToArray();
            for (int i = 0; i < events.Length; i++)
            {
                var eventInfo = events[i];                
                var match = expectedData.SingleOrDefault(o =>
                {
                    return o.LogLevel == eventInfo.Level &&
                           o.PropertiesPredicate(eventInfo.Properties) &&
                           o.Message.Equals(eventInfo.FormattedMessage);
                });

                Assert.NotNull(match);
            }
        }

        [Test]
        public void When_ScopeProperties_NoPropertiesSet()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var logProps = new { Key1 = "Value One", key2 = "Value Two" };

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName])                                                        
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(logProps.Key1)].Equals(logProps.Key1) &&
                                                        properties[nameof(logProps.key2)].Equals(logProps.key2)
                }
            };

            using (_logger.BeginScope(scopeName, logProps, new ScopeConfiguration{IncludeProperties = false}))
            {
                _logger.Extended(_logLevel, message, null);
            }

            var events = _events.ToArray();
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
        public void When_ScopeProperties_LoggerProperties_PropertiesSet()
        {
            const string scopeName = "empty scope";
            const string message = "hello world";
            var scopeProperties = new { Key1 = "Value One", key2 = "Value Two" };
            var loggerProperties = new {LoggerProperty = "Value Three"};

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(loggerProperties.LoggerProperty)].Equals(loggerProperties.LoggerProperty)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2)
                }
            };

            using (_logger.BeginScope(scopeName, scopeProperties))
            {
                _logger.Extended(_logLevel, message, loggerProperties);
            }

            var events = _events.ToArray();
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
        public void When_ScopeProperties_NestedScope_NoPropertiesSet()
        {
            const string scopeName = "empty scope";
            const string nestedScope = "nested scope";
            const string message = "hello world";
            var scopeProperties = new { Key1 = "Value One", key2 = "Value Two" };
            var nestedScopeProperties = new { NestedScopeProperty = "Value Four" };
            var loggerProperties = new { LoggerProperty = "Value Three" };
            var loggerPropertiesTwo = new { LoggerProperty = "Value Five" };
            string outerScopeId = null;

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScope) &&
                                                        properties[ScopeIdTracePropertyName].Equals($"{outerScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(nestedScopeProperties.NestedScopeProperty)].Equals(nestedScopeProperties.NestedScopeProperty)                               
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScope) &&
                                                        properties[ScopeIdTracePropertyName].Equals($"{outerScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(loggerProperties.LoggerProperty)].Equals(loggerProperties.LoggerProperty)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScope) &&
                                                        properties[ScopeIdTracePropertyName].Equals($"{outerScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(loggerPropertiesTwo.LoggerProperty)].Equals(loggerPropertiesTwo.LoggerProperty)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScope) &&
                                                        properties[ScopeIdTracePropertyName].Equals($"{outerScopeId} -> {properties[ScopeIdPropertyName]}") &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(nestedScopeProperties.NestedScopeProperty)].Equals(nestedScopeProperties.NestedScopeProperty)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeIdTracePropertyName].Equals(properties[ScopeIdPropertyName]) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2)
                }
            };

            using (var outerScope = _logger.BeginScope(scopeName, scopeProperties))
            {
                outerScopeId = outerScope.ScopeId.ToString();
                using (_logger.BeginScope(nestedScope, nestedScopeProperties, new ScopeConfiguration{IncludeProperties = false}))
                {
                    _logger.Extended(_logLevel, message, loggerProperties);
                    _logger.Extended(_logLevel, message, loggerPropertiesTwo);
                }
            }

            var events = _events.ToArray();
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
        public void When_ScopeProperties_NestedScope_InheritConfiguration()
        {
            const string scopeName = "empty scope";
            const string nestedScope = "nested scope";
            const string message = "hello world";
            var scopeProperties = new { Key1 = "Value One", key2 = "Value Two" };
            var nestedScopeProperties = new { NestedScopeProperty = "Value Four" };
            var loggerProperties = new { LoggerProperty = "Value Three" };
            var loggerPropertiesTwo = new { LoggerProperty = "Value Five" };
            string outerScopeName = null;

            var expectedData = new[]
            {
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeNameTracePropertyName].Equals(properties[ScopePropertyName]) &&
                                                        !properties.ContainsKey(ScopeIdTracePropertyName) && 
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = StartLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScope) &&
                                                        properties[ScopeNameTracePropertyName].Equals($"{outerScopeName} -> {properties[ScopePropertyName]}") &&
                                                        !properties.ContainsKey(ScopeIdTracePropertyName) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(nestedScopeProperties.NestedScopeProperty)].Equals(nestedScopeProperties.NestedScopeProperty)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScope) &&
                                                        properties[ScopeNameTracePropertyName].Equals($"{outerScopeName} -> {properties[ScopePropertyName]}") &&
                                                        !properties.ContainsKey(ScopeIdTracePropertyName) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(loggerProperties.LoggerProperty)].Equals(loggerProperties.LoggerProperty)
                },
                new ExpectedData
                {
                    LogLevel = _logLevel,
                    Message = message,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScope) &&
                                                        properties[ScopeNameTracePropertyName].Equals($"{outerScopeName} -> {properties[ScopePropertyName]}") &&
                                                        !properties.ContainsKey(ScopeIdTracePropertyName) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(loggerPropertiesTwo.LoggerProperty)].Equals(loggerPropertiesTwo.LoggerProperty)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(nestedScope) &&
                                                        properties[ScopeNameTracePropertyName].Equals($"{outerScopeName} -> {properties[ScopePropertyName]}") &&
                                                        !properties.ContainsKey(ScopeIdTracePropertyName) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2) &&
                                                        properties[nameof(nestedScopeProperties.NestedScopeProperty)].Equals(nestedScopeProperties.NestedScopeProperty)
                },
                new ExpectedData
                {
                    LogLevel = LogLevel.Trace,
                    Message = FinishLogicalScopeMessage,
                    PropertiesPredicate = properties => properties[ScopePropertyName].Equals(scopeName) &&
                                                        properties[ScopeNameTracePropertyName].Equals(properties[ScopePropertyName]) &&
                                                        !properties.ContainsKey(ScopeIdTracePropertyName) &&
                                                        properties[nameof(scopeProperties.Key1)].Equals(scopeProperties.Key1) &&
                                                        properties[nameof(scopeProperties.key2)].Equals(scopeProperties.key2)
                }
            };

            using (var outerScope = _logger.BeginScope(scopeName, scopeProperties, 
                new ScopeConfiguration{IncludeScopeIdTrace = false}))
            {
                outerScopeName = outerScope.Scope;
                using (_logger.BeginScope(nestedScope, nestedScopeProperties, 
                    new ScopeConfiguration{InheritConfiguration = true}))
                {
                    _logger.Extended(_logLevel, message, loggerProperties);
                    _logger.Extended(_logLevel, message, loggerPropertiesTwo);
                }
            }

            var events = _events.ToArray();
            for (int i = 0; i < events.Length; i++)
            {
                var eventInfo = events[i];
                var expected = expectedData[i];

                Assert.That(eventInfo.Level, Is.EqualTo(expected.LogLevel));
                Assert.That(expected.PropertiesPredicate(eventInfo.Properties));
                Assert.AreEqual(eventInfo.FormattedMessage, expected.Message);
            }
        }        
    }    
}

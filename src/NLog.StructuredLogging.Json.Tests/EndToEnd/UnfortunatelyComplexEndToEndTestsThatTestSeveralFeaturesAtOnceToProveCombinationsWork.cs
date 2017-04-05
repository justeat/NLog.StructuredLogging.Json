using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    public abstract class UnfortunatelyComplexEndToEndTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork : EndToEndTests
    {
        private Exception _exception;
        protected IList<string> Result;
        protected int Iterations;
        private IList<string> _control;
        protected IList<string> AttributesOnLogEvent;
        private IDictionary<string, string> _attributesNotYetAssertable;

        protected override void Given()
        {
            Iterations = 10;
            _exception = GivenException();
            AttributesOnLogEvent = GivenAttributesOnLogEvent();
            _attributesNotYetAssertable = GivenAttributesNotYetAssertable();
            Message = GivenMessage();
            base.Given();
        }

        protected override void ModifyLoggingConfigurationBeforeCommit(string nameOfTargetForSut, LoggingConfiguration config)
        {
            AddControlOutputToCheckAgainst(nameOfTargetForSut, config);
            base.ModifyLoggingConfigurationBeforeCommit(nameOfTargetForSut, config);
        }

        protected virtual IDictionary<string,string> GivenAttributesNotYetAssertable()
        {
            var result = new Dictionary<string, string>();

            if (!Env.HasCallSite)
            {
                result.Add("CallSite", "Cannot yet generate CallSite in dotNet core");
            }

            return result;

        }

        protected virtual IList<string> GivenAttributesOnLogEvent()
        {
            return new List<string>
            {
                "TimeStamp",
                "Level", "LoggerName", "Message",
                "ProcessId", "ThreadId",
                "CallSite",
                "Exception", "ExceptionType", "ExceptionMessage",
                "ExceptionStackTrace", "ExceptionFingerprint",
                "ExceptionIndex", "ExceptionCount",
                "PropertyOne", "PropertyTwo", "Iteration",
                "ex_key_1", "ex_key_2"
            };
        }

        protected virtual string GivenMessage()
        {
            #region long message with edge cases and control chars

            var message = @"
This is a message
!""£$%^&*

With lots of possibly bad things in it";

            // all the control chars
            message += '\u0001';
            message += '\u0002';
            message += '\u0003';
            message += '\u0004';
            message += '\u0005';
            message += '\u0006';
            message += '\u0007';
            message += '\u0008';
            message += '\u0009';
            message += '\u0010';
            message += '\u0011';
            message += '\u0012';
            message += '\u0013';
            message += '\u0014';
            message += '\u0015';
            message += '\u0016';
            message += '\u0017';
            message += '\u0018';
            message += '\u0019';

            #endregion

            return message;
        }

        private static Exception GivenException()
        {
            var testEx = new InvalidOperationException("Outer Exception");
            testEx.Data.Add("ex_key_1", "ex_data_1");
            testEx.Data.Add("ex_key_2", "ex_data_2");

            return PutStackTraceOnException(testEx);
        }

        private static Exception PutStackTraceOnException(Exception inputEx)
        {
            try
            {
                throw inputEx;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        protected override void When()
        {
            for (var i = 0; i < Iterations; i++)
            {
                Sut.ExtendedException(_exception, Message, new {PropertyOne = "one", PropertyTwo = 2, Iteration = i});
            }
            LogManager.Flush();
            Result = LogManager.Configuration.LogMessage(TargetName);
            _control = LogManager.Configuration.LogMessage("control" + TargetName);
        }

        private void AddControlOutputToCheckAgainst(string name, LoggingConfiguration config)
        {
            var layout = new JsonLayout { SuppressSpaces = true };
            foreach (var attribute in GivenControlOutputAttributes())
            {
                layout.Attributes.Add(attribute);
            }
            var target = new MemoryTarget { Name = "control" + name, Layout = layout };
            config.AddTarget(target);
            SetUpRules(target, config);
        }

        protected virtual IEnumerable<JsonAttribute> GivenControlOutputAttributes()
        {
            yield return new JsonAttribute("TimeStamp", "${date:format=yyyy-MM-ddTHH\\:mm\\:ss.fffZ}");
            yield return new JsonAttribute("Level", "${level}");
            yield return new JsonAttribute("LoggerName", "${logger}");
            yield return new JsonAttribute("Message", "${message}");
            yield return new JsonAttribute("ProcessId", "${processid}");
            yield return new JsonAttribute("ThreadId", "${threadid}");
            yield return new JsonAttribute("Parameters", "");
            yield return new JsonAttribute("CallSite", "${callsite}");
            yield return new JsonAttribute("PropertyOne", "one");
            yield return new JsonAttribute("PropertyTwo", "2");
            yield return new JsonAttribute("Iteration", "1");
            yield return new JsonAttribute("Exception", "${exception:format=ToString}");
            yield return new JsonAttribute("ExceptionType", "${exception:format=ShortType}");
            yield return new JsonAttribute("ExceptionMessage", "${exception:format=Message}");
            yield return new JsonAttribute("ExceptionStackTrace", "${exception:format=StackTrace}");
            yield return new JsonAttribute("ExceptionFingerprint", "${hasher:Inner=${exception:format=StackTrace}}");
            yield return new JsonAttribute("ExceptionIndex", "1");
            yield return new JsonAttribute("ExceptionCount", "1");
            yield return new JsonAttribute("ex_key_1", "ex_data_1");
            yield return new JsonAttribute("ex_key_2", "ex_data_2");
        }

        [Test]
        public void ShouldHaveLoggedTimeStampInIso8601Utc()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(
                    @"TimeStamp"":""\d{4,4}-\d{2,2}-\d{2,2}T\d{2,2}:\d{2,2}:\d{2,2}\.\d{3,3}Z");
            }
        }

        [Test]
        public void ShouldHaveLoggedCorrectTimeStamp()
        {
            foreach (var line in Result)
            {
                var jo = JObject.Parse(line);
                Assert.That(jo["TimeStamp"].ToObject<DateTime>(), Is.EqualTo(TimeSourceForTest.Time));
            }
        }

        [Test]
        public virtual void ShouldHaveExpectedNumberOfLines()
        {
            Assert.That(Result.Count, Is.EqualTo(Iterations));
        }

        [Test]
        public virtual void ShouldHaveExpectedNumberOfOpeningBraces()
        {
            var expected = GivenExpectedNumberBraces();
            var all = string.Concat(Result);
            Assert.That(all.Count(c => c == '{'), Is.EqualTo(expected));
        }

        [Test]
        public virtual void ShouldHaveExpectedNumberOfClosingBraces()
        {
            var expected = GivenExpectedNumberBraces();
            var all = string.Concat(Result);
            Assert.That(all.Count(c => c == '{'), Is.EqualTo(expected));
        }

        [Test]
        public void ShouldEndLineWithBrace()
        {
            foreach (var line in Result)
            {
                line.Last().ToString().ShouldBe("}");
            }
        }

        protected virtual int GivenExpectedNumberBraces()
        {
            return Iterations;
        }

        [Test]
        public void ShouldHaveLoggedAtErrorLevel()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch("Error");
            }
        }

        [Test]
        public void EachPropertyShouldMatchControlOutput()
        {
            Assert.That(_control.Count, Is.EqualTo(Iterations));

            for (var i = 0; i < Iterations; i++)
            {
                var actual = JToken.Parse(Result[i]);
                var control = JToken.Parse(_control[i]);

                foreach (var prop in AttributesOnLogEvent)
                {
                    if (_attributesNotYetAssertable.Any(x => x.Key.Equals(prop)))
                    {
                        Console.WriteLine("No support for {0} yet: {1}", prop, _attributesNotYetAssertable[prop]);
                        continue;
                    }
                    if (prop.Equals("ThreadId"))
                    {
                        Assert.That(actual[prop].Value<int>(), Is.GreaterThan(0));
                        continue;
                    }
                    if (prop.Equals("Iteration"))
                    {
                        Assert.That(actual[prop].Value<int>(), Is.GreaterThanOrEqualTo(0));
                        continue;
                    }

                    Assert.That(actual[prop], Is.EqualTo(control[prop]), () => AttributeMissingMessage(prop));
                }
            }
        }

        private string AttributeMissingMessage(string prop)
        {
            var propName = _attributesNotYetAssertable.ContainsKey(prop) ? _attributesNotYetAssertable[prop] : prop;
            return string.Format("The attribute {0} : '{1}' should be in attributes not assertable, or there's a test failure. Prefer making it assertable!",
                prop, propName);
        }

        [Test]
        public void ShouldNotHavePropertiesBesidesWhatIsExpected()
        {
            var calculatedCount = AttributesOnLogEvent.Count - _attributesNotYetAssertable.Count;

            var jsonRows = Result.Select(JToken.Parse)
                .ToList();

            foreach (var entry in jsonRows)
            {
                Assert.That(entry.Count(), Is.EqualTo(calculatedCount),
                    "Entry has props:" String.Join(",", entry.Select(t => t.ToString())));
            }
        }

        [Test]
        public void ShouldHaveLoggedWithLoggerName()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(LoggerName);
            }
        }

        [Test]
        public virtual void ShouldHaveLoggedAMessage()
        {
            foreach (var line in Result)
            {
                var obj = JObject.Parse(line);
                var message = obj.GetValue("Message").ToString();
                message.ShouldMatch("This is a message");
                message.ShouldMatch(@"!\""£\$%\^&\*");
                message.ShouldMatch("With lots of possibly bad things in it");
                message.ShouldMatch(@"This is a message\r\n!\"".*\$%\^&\*\r\n\r\nWith lots of possibly bad things in it");
            }
        }

        [Test]
        public void ShouldHaveLoggedInlineStructuredData()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(@"PropertyOne"":""one""");
                line.ShouldMatch(@"PropertyTwo"":""2""");
                line.ShouldMatch(@"Iteration"":""\d+""");
            }
        }

        [Test]
        public void ShouldHaveLoggedExceptionCorrectly()
        {
            foreach (var line in Result)
            {
                var obj = JObject.Parse(line);
                obj.GetValue("Exception").ToString().ShouldMatch(@"System\.InvalidOperationException: Outer Exception");
                obj.GetValue("ExceptionType").ToString().ShouldMatch("InvalidOperationException");
                obj.GetValue("ExceptionMessage").ToString().ShouldMatch("Outer Exception");
                obj.GetValue("ExceptionStackTrace").ToString().ShouldMatch("   at NLog.StructuredLogging.Json.Tests.EndToEnd.UnfortunatelyComplexEndToEndTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork.PutStackTraceOnException");
            }
        }

        [Test]
        [Category("CallSite")]
        public void ShouldHaveLoggedCallSite()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(@"CallSite"":""NLog\.StructuredLogging\.Json\.Tests\.EndToEnd\.UnfortunatelyComplexEndToEndTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork\.When");
            }
        }

        [Test]
        public void ShouldHaveLoggedExceptionDataBagCorrectly()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(@"""ex_key_1"":""ex_data_1");
                line.ShouldMatch(@"""ex_key_2"":""ex_data_2");
            }
        }

        [Test]
        public void ShouldStartWithOpeningBrace()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(@"^\{");
            }
        }

        [Test]
        public void ShouldEndWithClosingBrace()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(@"\}$");
            }
        }

        [Test]
        public void ShouldBeValidJson()
        {
            foreach (var line in Result)
            {
                Assert.DoesNotThrow(() => JToken.Parse(line));
            }
        }

        [Test]
        public void ShouldNotContainRawControlCharacters()
        {
            var controlCharacters = new[]
            {
                '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008', '\u0009', '\u0010',
                '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019'
            };
            foreach (var line in Result)
            {
                var chars = line.ToCharArray().ToArray();
                foreach (var bc in controlCharacters)
                {
                    var itsThere = chars.Any(ch => ch == bc);
                    Assert.That(itsThere, Is.False, "Contains bad char " + bc);
                }
            }
        }
    }

    public static class LogConfigExt
    {
        public static IList<string> LogMessage(this LoggingConfiguration config, string name)
        {
            var target = config.AllTargets.Single(x => x.Name.Equals(name)) as MemoryTarget;
            Debug.Assert(target != null, "target != null");
            return target.Logs;
        }
    }
}

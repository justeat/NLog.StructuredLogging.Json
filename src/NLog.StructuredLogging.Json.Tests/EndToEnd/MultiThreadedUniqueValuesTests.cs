using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    /// <summary>
    /// Log lots of messages with the same keys and differing values
    /// The test is that on any message logged, the guids should all be the same.
    /// If we observe a log entry where the guids differ, then it has come from a different log entry.
    /// </summary>
    public abstract class MultiThreadedUniqueValuesTests : EndToEndTests
    {
        private const int Threads = 200;
        private const int LogEntriesPerThread = 100;

        protected IList<string> Lines;

        protected override void When()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < Threads; i++)
            {
                var task = Task.Factory.StartNew(
                    async () => await LogSomeMessages())
                    .Unwrap();

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            Lines = LogManager.Configuration.LogMessage(TargetName);
        }

        private async Task LogSomeMessages()
        {
            for (var i = 0; i < LogEntriesPerThread; i++)
            {
                LogAMessage();
                await Task.Delay(1);
            }
        }

        private void LogAMessage()
        {
            var uniqueValue = Guid.NewGuid().ToString();

            var logInfo = new
            {
                Key0 = uniqueValue,
                Key1 = uniqueValue,
                Key2 = uniqueValue,
                Key3 = uniqueValue,
                Key4 = uniqueValue,
                Key5 = uniqueValue,
                Key6 = uniqueValue,
                Key7 = uniqueValue,
                Key8 = uniqueValue,
                Key9 = uniqueValue
            };

            Sut.ExtendedInfo("TestMessage with unique value:" + uniqueValue, logInfo);
        }

        [Test]
        public void OutputHasConsistentLines()
        {
            Assert.That(Lines, Is.Not.Empty);
            Assert.That(Lines.Count, Is.EqualTo(Threads * LogEntriesPerThread));

            foreach (var line in Lines)
            {
                LogEntryHasConsistentValues(line);
            }
        }

        private static void LogEntryHasConsistentValues(string logEntry)
        {
            Assert.That(logEntry, Is.Not.Empty);

            var parsed = JObject.Parse(logEntry);

            var message = parsed["Message"].ToString();

            var values = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                var value = parsed["Key" + i].ToString();

                values.Add(value);
            }

            var uniqueValues = values.Distinct().ToList();

            Assert.That(values.Count, Is.EqualTo(10));
            Assert.That(uniqueValues.Count, Is.EqualTo(1));

            var theLogMessageGuid = uniqueValues[0];

            Assert.That(message, Does.Contain(theLogMessageGuid));
        }
    }
}

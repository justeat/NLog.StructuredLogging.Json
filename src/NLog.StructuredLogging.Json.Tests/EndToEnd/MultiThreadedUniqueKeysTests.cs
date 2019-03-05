using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    /// <summary>
    /// Log lots of messages with differing keys
    /// The test is that new keys should not appear on a message. 
    /// If we observe a log entry where the guids differ or with extra keys,
    /// then it has come from a different log entry.
    /// </summary>
    public abstract class MultiThreadedUniqueKeysTests : EndToEndTests
    {
        private const int Threads = 200;
        private const int LogEntriesPerThread = 100;

        protected IList<string> Lines;

        protected override void When()
        {
            var tasks = new List<Task>();

            for (var i = 0; i < Threads; i++)
            {
                var startIndex = i * Threads;
                var task = Task.Factory.StartNew(
                    async () => await LogSomeMessages(startIndex))
                    .Unwrap();

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            Lines = LogManager.Configuration.LogMessage(TargetName);
        }

        private async Task LogSomeMessages(int startIndex)
        {
            for (int i = 0; i < LogEntriesPerThread; i++)
            {
                LogAMessage(startIndex + i);
                await Task.Delay(1);
            }
        }

        private void LogAMessage(int index)
        {
            var uniqueValue = Guid.NewGuid().ToString();
            var messageType = index % 4;
            var messageText = "Test message type " + messageType + " with value " + uniqueValue;

            switch (messageType)
            {
                case 0:
                    var logInfo0 = new
                    {
                        InvariantKey = uniqueValue,
                        Key0 = uniqueValue,
                        Key1 = uniqueValue,
                    };
                    Sut.ExtendedInfo(messageText, logInfo0);
                    break;

                case 1:
                    var logInfo1 = new
                    {
                        InvariantKey = uniqueValue,
                        Key2 = uniqueValue,
                        Key3 = uniqueValue,
                    };
                    Sut.ExtendedInfo(messageText, logInfo1);
                    break;

                case 2:
                    var logInfo2 = new
                    {
                        InvariantKey = uniqueValue,
                        Key4 = uniqueValue,
                        Key5 = uniqueValue,
                    };
                    Sut.ExtendedInfo(messageText, logInfo2);
                    break;

                case 3:
                    var logInfo3 = new
                    {
                        InvariantKey = uniqueValue,
                        Key6 = uniqueValue,
                        Key7 = uniqueValue,
                    };
                    Sut.ExtendedInfo(messageText, logInfo3);
                    break;
            }
        }

        [Test]
        public void OutputHasConsistentLines()
        {
            Assert.That(Lines, Is.Not.Empty);
            Assert.That(Lines.Count, Is.EqualTo(Threads * LogEntriesPerThread));

            foreach (var line in Lines)
            {
                LogEntryHasConsistentKeys(line);
            }
        }

        private static void LogEntryHasConsistentKeys(string logEntry)
        {
            Assert.That(logEntry, Is.Not.Empty);

            var parsed = JObject.Parse(logEntry);

            var message = parsed["Message"].ToString();
            var invariantValue = parsed["InvariantKey"].ToString();

            var values = new List<string>();

            foreach (var property in parsed.Properties())
            {
                if (property.Name.StartsWith("Key"))
                {
                    values.Add(property.Value.ToString());
                }
            }

            Assert.That(message, Is.Not.Empty);
            Assert.That(invariantValue, Is.Not.Empty);

            Assert.That(values.Count, Is.EqualTo(2));

            var uniqueValues = values.Distinct().ToList();

            Assert.That(uniqueValues.Count, Is.EqualTo(1));
            Assert.That(uniqueValues[0], Is.EqualTo(invariantValue));
        }
    }
}

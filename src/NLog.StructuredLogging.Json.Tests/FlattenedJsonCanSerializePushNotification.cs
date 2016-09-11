using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    public class FlattenedJsonCanSerializePushNotification
    {
        [Test]
        public void LogEntryIsValidJson()
        {
            string body;
            using (var sr = new StreamReader(@".\data\push-notification.json"))
            {
                body = sr.ReadLine();
            }

            var layout = new FlattenedJsonLayout();

            var logProperties = new { ConsumerId = "consumerId", Body = body };

            var log = new LogEventInfo(LogLevel.Info, "TheLoggerName", "Received message");

            var propertyDictionary = logProperties.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(logProperties, null));
            foreach (var element in propertyDictionary)
            {
                log.Properties.Add(element.Key, element.Value);
            }

            string result = layout.Render(log);

            var token = JToken.Parse(result);
            Assert.IsTrue(token.HasValues);
        }
    }
}

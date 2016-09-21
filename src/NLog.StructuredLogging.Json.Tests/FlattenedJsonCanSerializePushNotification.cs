using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    public class FlattenedJsonCanSerializePushNotification
    {
        private const string ExamplePushNotification =
            @"{""TimeStamp"":""2016-06-16T10:17:59.857Z"",""Level"":""Debug"",""LoggerName"":""lambda_method"",""Message"":""Push notification sent"",""ProcessId"":""2760"",""ThreadId"":""35"",""CallSite"":""Acme.Push.PushNotifier.LogPushNotificationSent"",""Body"":""{""default"":""Your order has been accepted"",""APNS"":""{\""aps\"":{\""alert\"":\""Your order has been accepted\"",\""sound\"":\""default\""},\""eid\"":\""abcdefg\"",\""et\"":\""order\"",\""es\"":\""accepted\"",\""t\"":\""2016-06-16T10:15:59.410004Z\""}"",""APNS_SANDBOX"":""{\""aps\"":{\""alert\"":\""Your order has been accepted\"",\""sound\"":\""default\""},\""eid\"":\""i4swocyfikkts7sktfqpoa\"",\""et\"":\""order\"",\""es\"":\""accepted\"",\""t\"":\""2016-06-16T10:15:59.410004Z\""}"",""GCM"":""{\""data\"":{\""message\"":\""Your order has been accepted\"",\""entityId\"":\""i4swocyfikkts7sktfqpoa\"",\""entityType\"":\""order\"",\""entityStatus\"":\""accepted\"",\""timeStamp\"":\""2016-06-16T10:15:59.410004Z\""}}"",""WNS"":""<toast launch = 'status,dsfgdfhdfghfg' >< visual >< binding template='ToastGeneric'><text>Just Eat Order Update</text><text>Your order has been accepted</text></binding></visual></toast>""}"",""ConsumerId"":""8862238"",""HttpStatusCode"":""OK"",""TargetArn"":""arn:aws:sns:eu-west-1:228773894774:endpoint/APNS/uk-production-consumernotifications-ios-live/abcdef-1234-3456-9abb-123456""}";

        [Test]
        public void LogEntryIsValidJson()
        {
            var layout = new FlattenedJsonLayout();

            var logProperties = new { ConsumerId = "consumerId", Body = ExamplePushNotification };

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

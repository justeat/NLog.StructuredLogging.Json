using System;
using System.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class SerializingJustSayingMessages
    {
        [Test]
        public void JustSayingMessagesSerializeFine()
        {
            var layoutRenderer = new StructuredLoggingLayoutRenderer();

            var logProperties = new FulfilmentStatusChangedNotificationRequested("some-order-id", "delivering", "thai-rice", "", "07839474638", 123, 234, 345);

            var log = new LogEventInfo(LogLevel.Info, "TheLoggerName", "Received message");
            
            var propertyDictionary = logProperties.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(logProperties, null));
            foreach (var element in propertyDictionary)
            {
                log.Properties.Add(element.Key, element.Value);
            }
            
            string result = layoutRenderer.Render(log);

            Assert.IsNotNullOrEmpty(result);
        }

        public class FulfilmentStatusChangedNotificationRequested : Message
        {
            public string OrderId { get; private set; }

            public int ConsumerId { get; private set; }

            public int RestaurantId { get; set; }

            public string State { get; private set; }

            public string RestaurantName { get; set; }

            public string StatusDetail { get; set; }

            public string PhoneNumber { get; set; }

            public int LegacyOrderId { get; set; }

            public FulfilmentStatusChangedNotificationRequested(string orderId, string state, string restaurantName, string statusDetail, string phoneNumber, int legacyOrderId, int consumerId, int restaurantId)
            {
                this.OrderId = orderId;
                this.State = state;
                this.RestaurantName = restaurantName;
                this.StatusDetail = statusDetail;
                this.PhoneNumber = phoneNumber;
                this.LegacyOrderId = legacyOrderId;
                this.ConsumerId = consumerId;
                this.RestaurantId = restaurantId;
            }
        }

        public abstract class Message
        {
            protected Message()
            {
                TimeStamp = DateTime.UtcNow;
                Id = Guid.NewGuid();
            }

            public Guid Id { get; set; }
            public DateTime TimeStamp { get; private set; }
            public string RaisingComponent { get; set; }
            public string Version { get; private set; }
            public string SourceIp { get; private set; }
            public string Tenant { get; set; }
            public string Conversation { get; set; }

            //footprint in order to avoid the same message being processed multiple times.
            public virtual string UniqueKey()
            {
                return Id.ToString();
            }
        }
    }
}
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    public abstract class DynamicLogPropertiesDoNotBleedToSubsequentLogEvents : EndToEndTests
    {
        private JObject _m1;
        private JObject _m2;

        protected override void When()
        {
            Sut.ExtendedInfo("message 1", new {foo = "bar", baz = "wibble"});
            Sut.ExtendedInfo("message 2", new {oddness = true});
            var lines = LogManager.Configuration.LogMessage(TargetName);
            _m1 = JObject.Parse(lines[0]);
            _m2 = JObject.Parse(lines[1]);
        }

        [Test]
        public void Message1HasExpectedDynamicProperties()
        {
            Assert.That(_m1["foo"].Value<string>(), Is.EqualTo("bar"));
            Assert.That(_m1["baz"].Value<string>(), Is.EqualTo("wibble"));
        }

        [Test]
        public void Message2HasExpectedDynamicProperties()
        {
            Assert.That(_m2["oddness"].Value<bool>(), Is.True);
        }

        [Test]
        public void Message2DoesNotHaveUnexpectedDynamicProperties()
        {
            FailIfPropertyPresent(_m2, "foo");
            FailIfPropertyPresent(_m2, "baz");
        }

        private static void FailIfPropertyPresent(JObject obj, string key)
        {
            JToken val;
            if (obj.TryGetValue(key, out val))
            {
                Assert.Fail($"object had key {key}, value was {val}");
            }
        }
    }
}

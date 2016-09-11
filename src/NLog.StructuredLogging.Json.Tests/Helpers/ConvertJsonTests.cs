using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog.StructuredLogging.Json.Helpers;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    [TestFixture]
    public class ConvertJsonTests
    {
        private const string ExpectedOutput =
            "{\"CamelCase\":\"testValue\",\"testKey\":1,\"Test_key2\":\"testValue2\"}";

        [Test]
        public void CanSerialiseToJsonAsExpected()
        {
            var data = MakeTestData();
            var result = ConvertJson.Serialize(data);

            Assert.That(result, Is.EqualTo(ExpectedOutput));
        }

        [Test]
        public void SerialisationIsNotAffectedByChangeToGlobalSettings()
        {
            var existingDefaults = JsonConvert.DefaultSettings;
            try
            {
                JsonConvert.DefaultSettings = ADifferentJsonSerializerSettings;

                var data = MakeTestData();
                var result = ConvertJson.Serialize(data);

                Assert.That(result, Is.EqualTo(ExpectedOutput));
            }
            finally
            {
                JsonConvert.DefaultSettings = existingDefaults;
            }
        }

        [Test]
        public void AChangeToGlobalSetttingGivesDifferentOutput()
        {
            var existingDefaults = JsonConvert.DefaultSettings;
            try
            {
                JsonConvert.DefaultSettings = ADifferentJsonSerializerSettings;

                var data = MakeTestData();
                var result = ConvertJson.Serialize(data);

                var dataAffectedBySetttings = JsonConvert.SerializeObject(data);

                Assert.That(result, Is.Not.EqualTo(dataAffectedBySetttings));
            }
            finally
            {
                JsonConvert.DefaultSettings = existingDefaults;
            }
        }

        private static JsonSerializerSettings ADifferentJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                PreserveReferencesHandling = PreserveReferencesHandling.None
            };
        }

        private Dictionary<string, object> MakeTestData()
        {
            return new Dictionary<string, object>
            {
                {"CamelCase", "testValue"},
                {"testKey", 1},
                {"Test_key2", "testValue2"}
            };
        }
    }
}

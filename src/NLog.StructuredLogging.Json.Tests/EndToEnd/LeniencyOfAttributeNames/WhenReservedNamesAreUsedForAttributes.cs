using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog.Layouts;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.LeniencyOfAttributeNames
{
    public abstract class WhenReservedNamesAreUsedForAttributes : EndToEndTests
    {
        private JObject _result;
        private Exception _exception;

        protected override void Given()
        {
            _exception = new Exception();
            _exception.Data.Add("TimeStamp", new DateTime(2016,1,2));
            base.Given();
        }

        protected override void When()
        {
            Sut.ExtendedException(_exception, "foo", new {TimeStamp = new DateTime(2016,1,1)});
            _result = JObject.Parse(LogManager.Configuration.LogMessage(TargetName).First());
        }

        [Test]
        public void NoExceptionWasThrown()
        {
            Assert.Pass("We would have failed in setup/when if one was thrown");
        }

        [Test]
        public void TheDuplicateAttributeNameShouldBePresentWithDataPrefix()
        {
            Assert.That(_result["data_TimeStamp"].ToObject<DateTime>(), Is.EqualTo(new DateTime(2016, 01, 01)));
        }

        [Test]
        public void TheDuplicateAttributeNameShouldBePresentWithExceptionPrefix()
        {
            Assert.That(_result["ex_TimeStamp"].ToObject<DateTime>(), Is.EqualTo(new DateTime(2016, 01, 02)));
        }
    }

    public class ViaLayout : WhenReservedNamesAreUsedForAttributes
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }

    public class ViaLayoutRenderer : WhenReservedNamesAreUsedForAttributes
    {
        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}

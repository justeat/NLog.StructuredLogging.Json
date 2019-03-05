using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog.Layouts;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ExceptionFingerprinting
{
    public abstract class NonExceptionsAreNotFingerprinted : EndToEndTests
    {
        protected JObject Result;

        protected override void When()
        {
            Sut.ExtendedInfo("foo", new { Bar = "baz" });
            var line = LogManager.Configuration.LogMessage(TargetName).First();
            Result = JObject.Parse(line);
        }

        [Test]
        public void ShouldNotHaveFingerprint()
        {
            JToken val;
            var gotValue = Result.TryGetValue("ExceptionFingerprint", StringComparison.Ordinal, out val);
            Assert.That(gotValue, Is.False);
        }
    }

    public class FlattenedJsonLayoutHasExceptionFingerprintingNotRenderWhenNoException : NonExceptionsAreNotFingerprinted
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }

    public class LayoutRendererHasExceptionFingerprintingNotRenderWhenNoException : NonExceptionsAreNotFingerprinted
    {
        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }

}

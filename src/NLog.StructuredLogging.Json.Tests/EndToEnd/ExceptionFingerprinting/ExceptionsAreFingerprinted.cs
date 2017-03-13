using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog.Layouts;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ExceptionFingerprinting
{
    public abstract class ExceptionsAreFingerprinted : EndToEndTests
    {
        protected Exception ThrownException;
        protected JObject Result;

        protected override void When()
        {
            Sut.ExtendedException(ThrownException, "boom!", null);
            var line = LogManager.Configuration.LogMessage(TargetName).First();
            Result = JObject.Parse(line);
        }

        protected override void Given()
        {
            ThrownException = GivenException();
            base.Given();
        }

        private static Exception GivenException()
        {
            var testEx = new InvalidOperationException("Outer Exception", new Exception("Inner Exception"));
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

        [Test]
        public void ShouldHaveFingerprint()
        {
            var value = Result["ExceptionFingerprint"].Value<string>();
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Is.Not.Empty);
        }
    }

    public class FlattenedJsonLayoutHasExceptionFingerprinting : ExceptionsAreFingerprinted
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }

    public class StructuredLoggingLayoutRendererHasExceptionFingerprinting : ExceptionsAreFingerprinted
    {
        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}

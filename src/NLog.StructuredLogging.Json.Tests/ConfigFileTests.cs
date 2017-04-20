using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class ConfigFileTests
    {
        [Test]
        public void ThrowExceptionsFlagShouldBeRead()
        {
            // throwExceptions="true" should have been read from nlog.config
            Assert.That(LogManager.ThrowExceptions, Is.True);
        }
    }
}
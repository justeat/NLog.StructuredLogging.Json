using System;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.JsonWithProperties
{
    [TestFixture]
    public class StructuredLoggingPropertyTests
    {
        public class Constructor
        {
            [TestCase(null)]
            [TestCase("")]
            [TestCase("  ")]
            public void DoesNotAllowEmptyName(object name)
            {
                Assert.Throws<ArgumentException>(
                    () => new StructuredLoggingProperty(name as string, "some-layout-value"));
            }

            [Test]
            public void AllowsNullLayoutValues()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new StructuredLoggingProperty("some-name", null));
            }
        }
    }
}

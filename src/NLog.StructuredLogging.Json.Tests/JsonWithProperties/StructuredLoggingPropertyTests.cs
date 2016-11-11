using System;
using NUnit.Framework;
using Shouldly;

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
                Action action = () => new StructuredLoggingProperty(name as string, "some-layout-value");
                action.ShouldThrow<ArgumentException>();
            }

            [Test]
            public void AllowsNullLayoutValues()
            {
                Action action = () => new StructuredLoggingProperty("some-name", null);
                action.ShouldThrow<ArgumentNullException>();
            }
        }
    }
}

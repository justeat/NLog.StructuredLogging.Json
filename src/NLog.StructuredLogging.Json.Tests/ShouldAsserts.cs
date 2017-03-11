using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    public static class ShouldAsserts
    {
        public static void ShouldBe(this string actual, string expected)
        {
            Assert.That(actual, Is.EqualTo(expected));
        }
        public static void ShouldContain(this string actual, string expected)
        {
            Assert.That(actual, Does.Contain(expected));
        }

        public static void ShouldMatch(this string actual, string expected)
        {
            Assert.That(actual, Does.Match(expected));
        }
    }
}

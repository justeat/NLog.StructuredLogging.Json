using NUnit.Framework;
using Shouldly;

namespace NLog.StructuredLogging.Json.Tests.Hashing
{
    [TestFixture]
    public class HasherTests
    {
        [Test]
        public void ForConverage()
        {
            var h = new HasherLayoutRenderer();
            h.Text = "{$date}";
            var output = h.Render(new LogEventInfo());
            output.ShouldNotBeNull();
        }
    }
}
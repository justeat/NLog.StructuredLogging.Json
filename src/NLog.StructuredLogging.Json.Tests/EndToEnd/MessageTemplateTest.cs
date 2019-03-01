using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    public abstract class MessageTemplateTest : EndToEndTests
    {
        private string _output;

        protected override void When()
        {
            var logInfo = new LogEventInfo
            {
                Level = LogLevel.Info,
                Message = "A message with {PropA} and {PropB} embedded",
                Parameters = new object[] {1, "two"}
            };

            Sut.Log(logInfo);
            var lines = LogManager.Configuration.LogMessage(TargetName);
            _output = lines[0];
        }

        [Test]
        public void OutputHasText()
        {
            Assert.That(_output, Does.Contain("\"Message\":\"A message with 1 and \\\"two\\\" embedded\""));
            Assert.That(_output, Does.Contain("\"MessageTemplate\":\"A message with {PropA} and {PropB} embedded"));
            Assert.That(_output, Does.Contain("\"PropA\":1"));
            Assert.That(_output, Does.Contain("\"PropB\":\"two"));
            Assert.That(_output, Does.Not.Contain("Parameters"));
        }
    }
}

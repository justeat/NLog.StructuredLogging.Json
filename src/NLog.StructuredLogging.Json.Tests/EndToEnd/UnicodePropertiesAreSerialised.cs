using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    public abstract class UnicodePropertiesAreSerialised : EndToEndTests
    {
        private string _output;

        protected override void When()
        {
            var logInfo = new
            {
                PlainText = "this is a text",
                PunctuationText = "!£$%^&*()_+:@~.,;",
                TextWithUnicode = "Unicode text: ß ðá åö лиц 我们 거리 αλεπού"
            };

            Sut.ExtendedInfo("testMessage", logInfo);
            var lines = LogManager.Configuration.LogMessage(TargetName);
            _output = lines[0];
        }

        [Test]
        public void OutputHasExpectedUnicodeText()
        {
            Assert.That(_output, Does.Contain("\"PlainText\":\"this is a text"));
            Assert.That(_output, Does.Contain("\"PunctuationText\":\"!£$%^&*()_+:@~.,;"));
            Assert.That(_output, Does.Contain("\"TextWithUnicode\":\"Unicode text: ß ðá åö лиц 我们 거리 αλεπού"));
        }
    }
}

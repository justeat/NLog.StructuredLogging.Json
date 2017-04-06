using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class LayoutRendererTests
    {
        public class ExampleParameters
        {
            public string ValueOne = "One";
            public int ValueTwo = 2;
        }

        public StructuredLoggingLayoutRenderer Renderer;
        public LogEventInfo LogEvent;
        public string Message { get; set; }
        public string Result;

        [SetUp]
        public void SetUp()
        {
            Message = @"
This is a message
!""£$%^&*

With lots of possibly bad things in it";

            // all the control chars
            Message += '\u0001';
            Message += '\u0002';
            Message += '\u0003';
            Message += '\u0004';
            Message += '\u0005';
            Message += '\u0006';
            Message += '\u0007';
            Message += '\u0008';
            Message += '\u0009';
            Message += '\u0010';
            Message += '\u0011';
            Message += '\u0012';
            Message += '\u0013';
            Message += '\u0014';
            Message += '\u0015';
            Message += '\u0016';
            Message += '\u0017';
            Message += '\u0018';
            Message += '\u0019';

            LogEvent = new LogEventInfo
            {
                Exception = new Exception("Outer Exception", new Exception("Inner Exception")),
                Level = LogLevel.Error,
                LoggerName = "ExampleLoggerName",
                Message = Message,
                Parameters = new object[] { "One", 1234 },
                Properties = { { "PropertyOne", "one" }, { "PropertyTwo", 2 } },
                TimeStamp = new DateTime(2014, 1, 2, 3, 4, 5, 623, DateTimeKind.Utc)
            };

            Renderer = new StructuredLoggingLayoutRenderer();
            Result = Renderer.Render(LogEvent);

            var outFileDir = "C:\\Temp";
            Directory.CreateDirectory(outFileDir);
            File.WriteAllLines(Path.Combine(outFileDir, "out.txt"), new[] { Result });
        }

        [Test]
        public void WhenConverted_TheResultIsNotEmpty()
        {
            Assert.That(Result, Is.Not.Empty);
        }

        [Test]
        public void WhenConverted_TheResultIsValidJson()
        {
            Assert.DoesNotThrow(() => JToken.Parse(Result));
        }

        [Test]
        public void WhenConverted_TheResultProducesTheCorrectJson()
        {
            var escapedLinebreak = (Environment.NewLine == "\n") ? "\\n" : "\\r\\n";

            var expected = 
                "{\"TimeStamp\":\"2014-01-02T03:04:05.623Z\"," +
                "\"Level\":\"Error\",\"LoggerName\":\"ExampleLoggerName\"," +
                "\"Message\":\"\\r\\nThis is a message\\r\\n!\\\"£$%^&*\\r\\n\\r\\nWith lots of possibly bad things in it" + 
                    "\\u0001\\u0002\\u0003\\u0004\\u0005\\u0006\\u0007\\b\\t\\u0010\\u0011\\u0012\\u0013\\u0014\\u0015\\u0016\\u0017\\u0018\\u0019\"," +
                $"\"Exception\":\"System.Exception: Outer Exception ---> System.Exception: Inner Exception{escapedLinebreak}   --- End of inner exception stack trace ---\"," +
                "\"ExceptionType\":\"Exception\","+
                "\"ExceptionMessage\":\"Outer Exception\"," +
                "\"ExceptionStackTrace\":null," +
                "\"ExceptionFingerprint\":\"55179621c796d669d13aee15725c01ba4524b44f\"," +
                "\"Parameters\":\"One,1234\",\"PropertyOne\":\"one\",\"PropertyTwo\":\"2\"}";


            Assert.That(Result, Is.EqualTo(expected));
        }

        [Test]
        public void WhenConverted_NoRawControlCharsArePresent()
        {
            var chars = Result.ToCharArray().ToArray();

            Assert.IsFalse(chars.Contains('\u0001'));
            Assert.IsFalse(chars.Contains('\u0002'));
            Assert.IsFalse(chars.Contains('\u0003'));
            Assert.IsFalse(chars.Contains('\u0004'));
            Assert.IsFalse(chars.Contains('\u0005'));
            Assert.IsFalse(chars.Contains('\u0006'));
            Assert.IsFalse(chars.Contains('\u0007'));
            Assert.IsFalse(chars.Contains('\u0008'));
            Assert.IsFalse(chars.Contains('\u0009'));
            Assert.IsFalse(chars.Contains('\u0010'));
            Assert.IsFalse(chars.Contains('\u0011'));
            Assert.IsFalse(chars.Contains('\u0012'));
            Assert.IsFalse(chars.Contains('\u0013'));
            Assert.IsFalse(chars.Contains('\u0014'));
            Assert.IsFalse(chars.Contains('\u0015'));
            Assert.IsFalse(chars.Contains('\u0016'));
            Assert.IsFalse(chars.Contains('\u0017'));
            Assert.IsFalse(chars.Contains('\u0018'));
            Assert.IsFalse(chars.Contains('\u0019'));
        }
    }
}

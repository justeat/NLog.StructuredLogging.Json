using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests
{
    [TestFixture]
    public class LayoutRendererCallSiteTests
    {
        public StructuredLoggingLayoutRenderer Renderer;
        public LogEventInfo LogEvent;
        public string Message { get; set; }
        public string Result;

        [SetUp]
        public void SetUp()
        {

            LogEvent = new LogEventInfo
            {
                Exception = new Exception(),
                Level = LogLevel.Error,
                LoggerName = "ExampleLoggerName",
                Message = "test message",
                TimeStamp = new DateTime(2014, 1, 2, 3, 4, 5, 623, DateTimeKind.Utc)
            };

            ThisNameWillAppearInTheCallSite(LogEvent);

            Renderer = new StructuredLoggingLayoutRenderer();
            Result = Renderer.Render(LogEvent);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThisNameWillAppearInTheCallSite(LogEventInfo logEvt)
        {
            ThisNameWillNotApppearinTheCallSite(logEvt);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThisNameWillNotApppearinTheCallSite(LogEventInfo logEvt)
        {
            logEvt.SetStackTrace(new StackTrace(1), 0);
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
        public void WhenConverted_TheResultProducesCallSiteInJson()
        {
            const string expectedPrefix =
                "{\"TimeStamp\":\"2014-01-02T03:04:05.623Z\",\"Level\":\"Error\",\"LoggerName\":\"ExampleLoggerName\",\"Message\":\"test message\"," +
                "\"Exception\":\"System.Exception: Exception of type 'System.Exception' was thrown.\",\"ExceptionType\":\"Exception\",\"ExceptionMessage\":\"Exception of type 'System.Exception' was thrown.\",\"ExceptionStackTrace\":null," +
                "\"ExceptionFingerprint\":\"b75c4fb74c46040a6c31534425178ba90541c17f\"," +
                "\"CallSite\":\"NLog.StructuredLogging.Json.Tests.LayoutRendererCallSiteTests.ThisNameWillAppearInTheCallSite";

            Assert.That(Result, Does.StartWith(expectedPrefix));
        }
    }
}

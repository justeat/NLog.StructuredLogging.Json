using System;
using NLog.Time;

namespace NLog.StructuredLogging.Json.Tests
{
    public class FakeTimeSource : TimeSource
    {
        private readonly DateTime _fakeTime;

        public FakeTimeSource()
        {
            _fakeTime = new DateTime(2014, 1, 2, 16, 4, 5, 623, DateTimeKind.Utc);
        }

        public override DateTime FromSystemTime(DateTime systemTime)
        {
            return _fakeTime;
        }

        public override DateTime Time { get { return _fakeTime; } }
    }
}

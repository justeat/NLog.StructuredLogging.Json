using System;
using BenchmarkDotNet.Attributes;
using NLog;
using NLog.StructuredLogging.Json;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class LoggingBenchmark
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly DateTimeOffset Timestamp = DateTimeOffset.UtcNow;
        private static readonly Exception Error = new InvalidOperationException("test ex");

        [Benchmark]
        public void LogInfo()
        {
            Logger.Info("standard message");
        }

        [Benchmark]
        public void LogExtendedInfo()
        {
            Logger.ExtendedInfo("standard message");
        }

        [Benchmark]
        public void LogExtendedInfoWithProperties()
        {
            Logger.ExtendedInfo("standard message",
                new { OrderId = 1234, orderState = "In progress", OrderPlacedAt = Timestamp });
        }

        [Benchmark]
        public void LogException()
        {
            Logger.Error(Error, "standard message");
        }

        [Benchmark]
        public void LogExtendedException()
        {
            Logger.ExtendedException(Error, "standard message");
        }

        [Benchmark]
        public void LogExceptionWithProperties()
        {
            Logger.ExtendedException(Error, "standard message",
                new { OrderId = 1234, orderState = "In progress", OrderPlacedAt = Timestamp });
        }
    }
}

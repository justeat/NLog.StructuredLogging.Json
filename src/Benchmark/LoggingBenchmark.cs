using System;
using BenchmarkDotNet.Attributes;
using NLog;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class LoggingBenchmark
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly DateTimeOffset Timestamp = DateTimeOffset.UtcNow;

        [Benchmark]
        public void LogInfoWithoutProperties()
        {
            Logger.Info("standard message");
        }

        [Benchmark]
        public void LogInfoWithProperties()
        {
            Logger.Info("standard message",
                new { OrderId = 1234, orderState = "In progress", OrderPlacedAt = Timestamp });
        }
    }
}

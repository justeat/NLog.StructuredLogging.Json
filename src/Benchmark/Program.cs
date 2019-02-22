using BenchmarkDotNet.Running;

namespace Benchmark
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args);
        }
    }
}

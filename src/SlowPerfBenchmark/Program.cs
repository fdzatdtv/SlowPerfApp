using BenchmarkDotNet.Running;

namespace SlowPerfBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<DateTimeBenchmark>();
            var summary = BenchmarkRunner.Run<ToUpperBenchmark>();
        }
    }
}

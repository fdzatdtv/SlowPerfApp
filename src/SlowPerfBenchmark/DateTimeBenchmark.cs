using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SlowPerfBenchmark
{
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class DateTimeBenchmark
    {
        private const int Times1 = 100_000;
        private const int Times2 = 1_000;

        [Benchmark]
        public void DateTimeNow()
        {
            for (int i = 0; i < Times1; i++)
            {
                var dt = DateTime.Now;
            }
        }

        [Benchmark]
        public void DateTimeUtcNow()
        {
            for (int i = 0; i < Times1; i++)
            {
                var dt = DateTime.UtcNow;
            }
        }

        [Benchmark]
        public void ForEachDaySlow()
        {
            var dt1 = new DateTime(2020,1,1);
            var dt2 = new DateTime(2020,12,31);

            for (int i = 0; i < Times2; i++)
            {
                for (DateTime dt = dt1.Date; dt < dt2.Date; dt = dt.AddDays(1).Date)
                {
                    int index = (dt - dt1).Days;
                }
            }
        }

        [Benchmark]
        public void ForEachDayFast()
        {
            var dt1 = new DateTime(2020,1,1).Date;
            var dt2 = new DateTime(2020,12,31).Date;

            for (int i = 0; i < Times2; i++)
            {
                for (DateTime dt = dt1; dt < dt2; dt = dt.AddDays(1))
                {
                    int index = (dt - dt1).Days;
                }
            }
        }
    }
}

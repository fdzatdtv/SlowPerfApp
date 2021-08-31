using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SlowPerfBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class ToUpperBenchmark
    {
        [Params(1_000)]
        public int N;

        private string Key1 = "306045b9-aec7-417e-a0df-5c1a5450a936";
        private string Key2 = "306045B9-AEC7-417E-A0DF-5C1A5450A936";

        [Benchmark(Baseline = true)]
        public void ToUpper()
        {
            for (int i = 0; i < N; i++)
            {
                bool isEqual = AreEqualWithToUpper(Key1, Key2);
                if (!isEqual)
                    break;
            }
        }

        private static bool AreEqualWithToUpper(string x, string y)
        {
            return x.ToUpper().Equals(y.ToUpper());
        }

        [Benchmark]
        public void CurrentCultureIgnoreCase()
        {
            for (int i = 0; i < N; i++)
            {
                bool isEqual = AreEqualWithIgnoreCaseCurrentCulture(Key1, Key2);
                if (!isEqual)
                    break;
            }
        }

        [Benchmark]
        public void OrdinalIgnoreCase()
        {
            for (int i = 0; i < N; i++)
            {
                bool isEqual = AreEqualWithIgnoreCase(Key1, Key2);
                if (!isEqual)
                    break;
            }
        }

        private static bool AreEqualWithIgnoreCase(string x, string y)
        {
            return string.Compare(x,y, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private static bool AreEqualWithIgnoreCaseCurrentCulture(string x, string y)
        {
            return string.Compare(x,y, StringComparison.CurrentCultureIgnoreCase) == 0;
        }
    }
}

using BenchmarkDotNet.Attributes;
using PasswordGenerator;

namespace PasswordGenerator.Benchmarks
{
    [MemoryDiagnoser]
    public class PasswordBenchmarks
    {
        [Params(1, 100, 1000, 10000)]
        public int Count;

        [Benchmark]
        public string SingleNext()
        {
            var pwd = new Password();
            return pwd.Next();
        }

        [Benchmark]
        public int Batch()
        {
            var pwd = new Password();
            var generated = 0;
            foreach (var _ in pwd.NextGroup(Count))
                generated++;
            return generated;
        }
    }
}

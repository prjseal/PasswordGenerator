using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace PasswordGenerator.Benchmarks
{
    /// <summary>Cost of generating a batch of passwords in one call.</summary>
    [MemoryDiagnoser]
    public class BatchGenerationBenchmarks
    {
        private Password _password = null!;

        [Params(1, 10, 100, 1000, 10000)]
        public int Count { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _password = new Password();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _password.Dispose();
        }

        [Benchmark]
        public IReadOnlyList<string> Generate()
        {
            return _password.Generate(Count);
        }
    }
}

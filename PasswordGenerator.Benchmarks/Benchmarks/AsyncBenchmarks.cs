using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace PasswordGenerator.Benchmarks
{
    /// <summary>Cost of the async generation APIs relative to their synchronous counterparts.</summary>
    [MemoryDiagnoser]
    public class AsyncBenchmarks
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
        public ValueTask<string> NextAsync()
        {
            return _password.NextAsync();
        }

        [Benchmark]
        public ValueTask<IReadOnlyList<string>> GenerateAsync()
        {
            return _password.GenerateAsync(Count);
        }
    }
}

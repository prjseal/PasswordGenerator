using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace PasswordGenerator.Benchmarks
{
    /// <summary>
    ///     Compares ways of producing N passwords in v3. The naive loop-over-<see cref="Password.Next" />
    ///     pattern (how v2 callers typically batched) is the baseline; the v3 batch
    ///     <see cref="Password.Generate(int)" /> API is measured against it.
    /// </summary>
    /// <remarks>
    ///     A true v2-vs-v3 comparison cannot run in one assembly: the published v2 package and the v3
    ///     project both produce <c>PasswordGenerator.dll</c>, so they collide in a single bin folder.
    /// </remarks>
    [MemoryDiagnoser]
    public class VersionComparisonBenchmarks
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

        [Benchmark(Baseline = true)]
        public int LoopNext()
        {
            var generated = 0;
            for (var i = 0; i < Count; i++)
            {
                _ = _password.Next();
                generated++;
            }

            return generated;
        }

        [Benchmark]
        public IReadOnlyList<string> BatchGenerate()
        {
            return _password.Generate(Count);
        }
    }
}

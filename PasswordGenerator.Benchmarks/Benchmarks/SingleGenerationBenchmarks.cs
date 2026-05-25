using BenchmarkDotNet.Attributes;

namespace PasswordGenerator.Benchmarks
{
    /// <summary>Cost of generating a single password across a range of lengths.</summary>
    [MemoryDiagnoser]
    public class SingleGenerationBenchmarks
    {
        private Password _password = null!;

        [Params(8, 16, 32, 64, 128)]
        public int Length { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _password = new Password(Length);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _password.Dispose();
        }

        [Benchmark]
        public string Next()
        {
            return _password.Next();
        }
    }
}

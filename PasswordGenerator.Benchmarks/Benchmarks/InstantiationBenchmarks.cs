using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace PasswordGenerator.Benchmarks
{
    /// <summary>Overhead of resolving a generator from the DI container versus constructing one directly.</summary>
    [MemoryDiagnoser]
    public class InstantiationBenchmarks
    {
        private ServiceProvider _provider = null!;

        [GlobalSetup]
        public void Setup()
        {
            _provider = new ServiceCollection()
                .AddPasswordGenerator()
                .BuildServiceProvider();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _provider.Dispose();
        }

        [Benchmark(Baseline = true)]
        public string DirectInstantiation()
        {
            using var password = new Password();
            return password.Next();
        }

        [Benchmark]
        public string ResolveFromContainer()
        {
            return _provider.GetRequiredService<IPasswordGenerator>().Next();
        }
    }
}

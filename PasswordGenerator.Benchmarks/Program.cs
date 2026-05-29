using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace PasswordGenerator.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // DefaultConfig already supplies the GitHub markdown exporter (MarkdownExporter-github),
            // which produces the *-report-github.md files the workflow drops into the step summary.
            // Benchmarks run on .NET 10, the runtime the library is built and shipped against.
            var config = DefaultConfig.Instance
                .AddDiagnoser(MemoryDiagnoser.Default)
                .AddJob(Job.Default.WithRuntime(CoreRuntime.Core10_0));

            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args, config);
        }
    }
}

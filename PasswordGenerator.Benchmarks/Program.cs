using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace PasswordGenerator.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // DefaultConfig already supplies the GitHub markdown exporter (MarkdownExporter-github),
            // which produces the *-report-github.md files the workflow drops into the step summary.
            var config = DefaultConfig.Instance
                .AddDiagnoser(MemoryDiagnoser.Default);

            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args, config);
        }
    }
}

using BenchmarkDotNet.Attributes;

namespace PasswordGenerator.Benchmarks
{
    /// <summary>Cost of generating from each built-in preset.</summary>
    [MemoryDiagnoser]
    public class PresetBenchmarks
    {
        private IPassword _owasp = null!;
        private IPassword _nist = null!;
        private IPassword _otp = null!;
        private IPassword _apiKey = null!;
        private IPassword _environmentName = null!;
        private IPasswordGenerator _passphrase = null!;

        [GlobalSetup]
        public void Setup()
        {
            _owasp = Password.ForOwasp();
            _nist = Password.ForNist();
            _otp = Password.ForOtp();
            _apiKey = Password.ForApiKey();
            _environmentName = Password.ForEnvironmentName();
            _passphrase = Password.ForPassphrase();
        }

        [Benchmark]
        public string ForOwasp() => _owasp.Next();

        [Benchmark]
        public string ForNist() => _nist.Next();

        [Benchmark]
        public string ForOtp() => _otp.Next();

        [Benchmark]
        public string ForApiKey() => _apiKey.Next();

        [Benchmark]
        public string ForEnvironmentName() => _environmentName.Next();

        [Benchmark]
        public string ForPassphrase() => _passphrase.Next();
    }
}

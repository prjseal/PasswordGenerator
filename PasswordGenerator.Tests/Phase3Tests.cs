using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    public class Phase3Tests
    {
        [Test]
        public async Task NextAsync_ReturnsPasswordOfConfiguredLength()
        {
            var pwd = new Password(20);
            var result = await pwd.NextAsync();
            Assert.That(result.Length, Is.EqualTo(20));
        }

        [Test]
        public void NextAsync_WithAlreadyCancelledToken_Throws()
        {
            var pwd = new Password(20);
            var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.That(async () => await pwd.NextAsync(cts.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public void NextAsync_WithCancelledToken_SurfacesCancellationThroughTask()
        {
            var pwd = new Password(20);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Cancellation must come back through the returned task, not as a synchronous throw,
            // so the result composes correctly when not awaited immediately.
            var task = pwd.NextAsync(cts.Token);
            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public void GenerateAsync_WithCancelledToken_SurfacesCancellationThroughTask()
        {
            var pwd = new Password(16);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task = pwd.GenerateAsync(10, cts.Token);
            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public void Generate_ReturnsRequestedCount()
        {
            var pwd = new Password(16);
            IReadOnlyList<string> result = pwd.Generate(25);
            Assert.That(result.Count, Is.EqualTo(25));
        }

        [Test]
        public void Generate_WithNegativeCount_Throws()
        {
            var pwd = new Password(16);
            Assert.Throws<ArgumentOutOfRangeException>(() => pwd.Generate(-1));
        }

        [Test]
        public async Task GenerateAsync_ReturnsRequestedCount()
        {
            var pwd = new Password(16);
            var result = await pwd.GenerateAsync(10);
            Assert.That(result.Count, Is.EqualTo(10));
        }

        [Test]
        public void Di_AddPasswordGenerator_WithCodeConfig_ResolvesAndGenerates()
        {
            var services = new ServiceCollection();
            services.AddPasswordGenerator(o =>
            {
                o.Length = 20;
                o.IncludeSpecial = false;
            });

            using var provider = services.BuildServiceProvider();
            var generator = provider.GetRequiredService<IPasswordGenerator>();

            var result = generator.Next();
            Assert.That(result.Length, Is.EqualTo(20));
            Assert.That(result.All(char.IsLetterOrDigit), Is.True, result);
        }

        [Test]
        public void Di_AddPasswordGenerator_BindsFromConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Length"] = "24",
                    ["IncludeSpecial"] = "false"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddPasswordGenerator(configuration);

            using var provider = services.BuildServiceProvider();
            var generator = provider.GetRequiredService<IPasswordGenerator>();

            Assert.That(generator.Next().Length, Is.EqualTo(24));
        }

        [Test]
        public void Di_ResolvedGenerator_BehavesLikeDirectConstruction()
        {
            var services = new ServiceCollection();
            services.AddPasswordGenerator(o => o.Length = 32);
            using var provider = services.BuildServiceProvider();

            var resolved = provider.GetRequiredService<IPasswordGenerator>();
            var direct = new Password(true, true, true, true, 32);

            Assert.That(resolved.Next().Length, Is.EqualTo(direct.Next().Length));
        }
    }
}

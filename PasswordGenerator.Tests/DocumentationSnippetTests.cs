using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    /// <summary>
    ///     Compile-and-run guards for the snippets in Readme.md and the v2->v3 migration guide, so the
    ///     documentation cannot drift from the public API.
    /// </summary>
    public class DocumentationSnippetTests
    {
        [Test]
        public void Readme_NextThrows_TryNextDoesNot()
        {
            // Next() throws ArgumentException on invalid settings.
            Assert.Throws<ArgumentException>(() => new Password(2).Next());

            // TryNext never throws.
            Assert.That(new Password(2).TryNext(out var bad), Is.False);
            Assert.That(bad, Is.Null);
            Assert.That(new Password(16).TryNext(out var ok), Is.True);
            Assert.That(ok, Is.Not.Null);
        }

        [Test]
        public async Task Readme_AsyncAndBatch()
        {
            var pwd = new Password(16);

            var password = await pwd.NextAsync(CancellationToken.None);
            Assert.That(password, Has.Length.EqualTo(16));

            IReadOnlyList<string> ten = pwd.Generate(10);
            Assert.That(ten, Has.Count.EqualTo(10));

            IReadOnlyList<string> ten2 = await pwd.GenerateAsync(10, CancellationToken.None);
            Assert.That(ten2, Has.Count.EqualTo(10));
        }

        [Test]
        public void MigrationGuide_SectionBinding_CodeOverridesConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["PasswordGenerator:Length"] = "16",
                    ["PasswordGenerator:IncludeSpecial"] = "true"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddPasswordGenerator(configuration.GetSection("PasswordGenerator"), o => o.Length = 24);

            using var provider = services.BuildServiceProvider();
            var generator = provider.GetRequiredService<IPasswordGenerator>();

            Assert.That(generator.Next(), Has.Length.EqualTo(24));
        }

        [Test]
        public void Readme_QualityControlsAndEntropy()
        {
            var custom = new Password().WithCharacters("ABCDEF0123456789").LengthRequired(24).Next();
            Assert.That(custom, Has.Length.EqualTo(24));

            var ascii = new Password().WithAllAscii().LengthRequired(40).Next();
            Assert.That(ascii, Has.Length.EqualTo(40));

            Assert.That(new Password(20).EstimateEntropyBits(), Is.GreaterThan(0));
        }
    }
}

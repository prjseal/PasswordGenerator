using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    public class PassphraseTests
    {
        [Test]
        public void WordCountForEntropy_DerivesEnoughWordsToMeetTarget()
        {
            // 7,776-word list => ~12.925 bits/word; with the trailing number 6 words clears 80 bits,
            // without it 7 are needed.
            Assert.That(PassphraseGenerator.WordCountForEntropy(80, includeNumber: true), Is.EqualTo(6));
            Assert.That(PassphraseGenerator.WordCountForEntropy(80, includeNumber: false), Is.EqualTo(7));
        }

        [Test]
        public void WordCountForEntropy_NeverReturnsLessThanOne()
        {
            Assert.That(PassphraseGenerator.WordCountForEntropy(0), Is.EqualTo(1));
            Assert.That(PassphraseGenerator.WordCountForEntropy(-50), Is.EqualTo(1));
        }

        [Test]
        public void ForPassphraseWithEntropy_MeetsOrExceedsTarget()
        {
            var generator = Password.ForPassphraseWithEntropy(80);
            Assert.That(generator.EstimateEntropyBits(), Is.GreaterThanOrEqualTo(80));
        }

        [Test]
        public void ForPassphraseWithEntropy_ProducesDerivedWordCount()
        {
            var generator = Password.ForPassphraseWithEntropy(80, separator: '.', includeNumber: true);
            var parts = generator.Next().Split('.');
            Assert.That(parts.Length, Is.EqualTo(7)); // 6 words + trailing number
        }

        [Test]
        public void EntropyFloor_RejectsWeakConfiguration()
        {
            Assert.Throws<ArgumentException>(() => Password.ForPassphrase(words: 2, minimumEntropyBits: 80));
        }

        [Test]
        public void EntropyFloor_AllowsStrongConfiguration()
        {
            Assert.DoesNotThrow(() => Password.ForPassphrase(words: 8, minimumEntropyBits: 80));
        }

        [Test]
        public void EntropyFloor_ZeroMeansNoEnforcement()
        {
            Assert.DoesNotThrow(() => Password.ForPassphrase(words: 1, minimumEntropyBits: 0));
        }

        [Test]
        public void EstimateEntropyBits_IsAvailableThroughInterface()
        {
            IPasswordGenerator generator = Password.ForPassphrase(6);
            Assert.That(generator.EstimateEntropyBits(), Is.GreaterThan(0));
        }

        private static readonly char[] SymbolChars = "!@#$%&*?".ToCharArray();

        [Test]
        public void IncludeSymbol_InjectsASymbol()
        {
            var generator = Password.ForPassphrase(4, separator: '.', includeNumber: false,
                includeSymbol: true);
            var phrase = generator.Next();
            Assert.That(phrase.IndexOfAny(SymbolChars), Is.GreaterThanOrEqualTo(0), phrase);
        }

        [Test]
        public void NumberAndSymbol_SatisfyCompositionRules()
        {
            var generator = Password.ForPassphrase(4, separator: '.', includeNumber: true,
                includeSymbol: true);
            var phrase = generator.Next();
            Assert.That(phrase.Any(char.IsDigit), Is.True, phrase);
            Assert.That(phrase.IndexOfAny(SymbolChars), Is.GreaterThanOrEqualTo(0), phrase);
        }

        [Test]
        public void IncludeSymbol_IsOffByDefault()
        {
            var generator = Password.ForPassphrase(4, separator: '.', includeNumber: false);
            var phrase = generator.Next();
            Assert.That(phrase.IndexOfAny(SymbolChars), Is.EqualTo(-1), phrase);
        }

        [Test]
        public void IncludeSymbol_AddsEntropy()
        {
            var withoutSymbol = Password.ForPassphrase(6, includeSymbol: false).EstimateEntropyBits();
            var withSymbol = Password.ForPassphrase(6, includeSymbol: true).EstimateEntropyBits();
            Assert.That(withSymbol, Is.GreaterThan(withoutSymbol));
        }

        [Test]
        public void ForMemorable_IsCapitalizedAndStrong()
        {
            var generator = Password.ForMemorable();
            Assert.That(generator.EstimateEntropyBits(), Is.GreaterThanOrEqualTo(80));
            var phrase = generator.Next();
            Assert.That(char.IsUpper(phrase[0]), Is.True, phrase);
        }

        [Test]
        public void Di_CodeConfiguresPassphrase()
        {
            var services = new ServiceCollection();
            services.AddPasswordGenerator(o =>
                o.Passphrase = new PassphraseOptions { WordCount = 6, Separator = '.' });

            using var provider = services.BuildServiceProvider();
            var generator = provider.GetRequiredService<IPasswordGenerator>();

            var parts = generator.Next().Split('.');
            Assert.That(parts.Length, Is.EqualTo(7)); // 6 words + trailing number (on by default)
        }

        [Test]
        public void Di_BindsPassphraseFromConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Passphrase:WordCount"] = "5",
                    ["Passphrase:Separator"] = ".",
                    ["Passphrase:IncludeNumber"] = "false"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddPasswordGenerator(configuration);

            using var provider = services.BuildServiceProvider();
            var generator = provider.GetRequiredService<IPasswordGenerator>();

            var parts = generator.Next().Split('.');
            Assert.That(parts.Length, Is.EqualTo(5)); // 5 words, no trailing number
        }
    }
}

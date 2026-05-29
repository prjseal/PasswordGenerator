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
        // A deterministic random source that cycles a fixed sequence, so generation is reproducible.
        private class FixedRandomSource : IRandomSource
        {
            private readonly int[] _values;
            private int _index;

            public FixedRandomSource(params int[] values) => _values = values;

            public int NextInt(int maxExclusive)
            {
                var value = _values[_index % _values.Length] % maxExclusive;
                _index++;
                return value;
            }
        }

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
        public void Next_SelectsWordsByRandomIndex()
        {
            var rng = new FixedRandomSource(0, 1, 2, 3);
            var generator = new PassphraseGenerator(4, '.', capitalize: false, includeNumber: false,
                includeSymbol: false, minimumEntropyBits: 0, randomSource: rng);

            var expected = string.Join('.',
                WordList.Words[0], WordList.Words[1], WordList.Words[2], WordList.Words[3]);
            Assert.That(generator.Next(), Is.EqualTo(expected));
        }

        [Test]
        public void Next_CapitalizesEachWordsFirstLetter()
        {
            var rng = new FixedRandomSource(0);
            var generator = new PassphraseGenerator(1, '.', capitalize: true, includeNumber: false,
                includeSymbol: false, minimumEntropyBits: 0, randomSource: rng);

            var word = WordList.Words[0];
            var expected = char.ToUpperInvariant(word[0]) + word.Substring(1);
            Assert.That(generator.Next(), Is.EqualTo(expected));
        }

        [Test]
        public void Next_PlacesSymbolOnTheChosenWord()
        {
            // Draw order: symbol-word index, symbol char index, then one index per word.
            var rng = new FixedRandomSource(0, 0, 0, 1);
            var generator = new PassphraseGenerator(2, '.', capitalize: false, includeNumber: false,
                includeSymbol: true, minimumEntropyBits: 0, randomSource: rng);

            // Symbol index 0 maps to '!' (first of "!@#$%&*?").
            var expected = WordList.Words[0] + "!" + "." + WordList.Words[1];
            Assert.That(generator.Next(), Is.EqualTo(expected));
        }

        [Test]
        public void Next_SamplesBroadlyAcrossTheWordList()
        {
            var generator = new PassphraseGenerator(1, '.', capitalize: false, includeNumber: false);

            var seen = new HashSet<string>();
            for (var i = 0; i < 2000; i++) seen.Add(generator.Next());

            // With 7,776 words and 2,000 draws the distinct count is ~1,700; >500 confirms broad,
            // non-degenerate sampling without being flaky.
            Assert.That(seen.Count, Is.GreaterThan(500));
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

        [Test]
        public void Next_WithNullSeparator_ConcatenatesWordsDirectly()
        {
            var rng = new FixedRandomSource(0, 1, 2, 3);
            var generator = new PassphraseGenerator(4, separator: null, capitalize: false,
                includeNumber: false, includeSymbol: false, minimumEntropyBits: 0, randomSource: rng);

            var expected = string.Concat(
                WordList.Words[0], WordList.Words[1], WordList.Words[2], WordList.Words[3]);
            Assert.That(generator.Next(), Is.EqualTo(expected));
        }

        [Test]
        public void Next_WithNullSeparator_AppendsNumberWithoutSeparator()
        {
            // Draw order: one index per word, then the trailing number (NextInt(90) + 10).
            var rng = new FixedRandomSource(0, 1, 5);
            var generator = new PassphraseGenerator(2, separator: null, capitalize: false,
                includeNumber: true, includeSymbol: false, minimumEntropyBits: 0, randomSource: rng);

            var expected = WordList.Words[0] + WordList.Words[1] + "15"; // 5 % 90 + 10
            Assert.That(generator.Next(), Is.EqualTo(expected));
        }

        [Test]
        public void Next_WithNullSeparator_StillCapitalizesEachWord()
        {
            var rng = new FixedRandomSource(0, 1);
            var generator = new PassphraseGenerator(2, separator: null, capitalize: true,
                includeNumber: false, includeSymbol: false, minimumEntropyBits: 0, randomSource: rng);

            static string Cap(string w) => char.ToUpperInvariant(w[0]) + w.Substring(1);
            var expected = Cap(WordList.Words[0]) + Cap(WordList.Words[1]);
            Assert.That(generator.Next(), Is.EqualTo(expected));
        }

        [Test]
        public void ForPassphrase_AcceptsNullSeparator()
        {
            var generator = (PassphraseGenerator)Password.ForPassphrase(4, separator: null);
            Assert.That(generator.Separator, Is.Null);
        }

        [Test]
        public void NullSeparator_DoesNotChangeEntropy()
        {
            var withSeparator = Password.ForPassphrase(6, separator: '-').EstimateEntropyBits();
            var withoutSeparator = Password.ForPassphrase(6, separator: null).EstimateEntropyBits();
            Assert.That(withoutSeparator, Is.EqualTo(withSeparator));
        }

        [Test]
        public void Di_BindsEmptySeparatorAsNull()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Passphrase:WordCount"] = "3",
                    ["Passphrase:Separator"] = "",
                    ["Passphrase:IncludeNumber"] = "false"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddPasswordGenerator(configuration);

            using var provider = services.BuildServiceProvider();
            var generator = (PassphraseGenerator)provider.GetRequiredService<IPasswordGenerator>();

            Assert.That(generator.Separator, Is.Null);
        }
    }
}

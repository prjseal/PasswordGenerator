using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordGenerator
{
    /// <summary>
    ///     Generates diceware-style passphrases from a built-in word list. Created via
    ///     <see cref="Password.ForPassphrase" />.
    /// </summary>
    public class PassphraseGenerator : IPasswordGenerator, IDisposable
    {
        private readonly IRandomSource _random;
        private readonly bool _ownsRandom;

        public PassphraseGenerator(int wordCount = 4, char separator = '-', bool capitalize = false,
            bool includeNumber = true, IRandomSource? randomSource = null)
        {
            if (wordCount < 1)
                throw new ArgumentOutOfRangeException(nameof(wordCount), "A passphrase needs at least one word.");

            WordCount = wordCount;
            Separator = separator;
            Capitalize = capitalize;
            IncludeNumber = includeNumber;
            _random = randomSource ?? new CryptoRandomSource();
            _ownsRandom = randomSource == null;
        }

        public int WordCount { get; }
        public char Separator { get; }
        public bool Capitalize { get; }
        public bool IncludeNumber { get; }

        /// <summary>The number of passphrases produced by the parameterless <see cref="Generate()" /> overload.</summary>
        public int DefaultBatchCount { get; set; } = 1;

        public string Next()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < WordCount; i++)
            {
                if (i > 0) sb.Append(Separator);

                var word = WordList.Words[_random.NextInt(WordList.Words.Length)];
                if (Capitalize && word.Length > 0)
                {
                    sb.Append(char.ToUpper(word[0], CultureInfo.InvariantCulture));
                    sb.Append(word, 1, word.Length - 1);
                }
                else
                {
                    sb.Append(word);
                }
            }

            if (IncludeNumber)
            {
                sb.Append(Separator);
                sb.Append((_random.NextInt(90) + 10).ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        public bool TryNext(out string? password)
        {
            password = Next();
            return true;
        }

        public Task<string> NextAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Next());
        }

        public IReadOnlyList<string> Generate()
        {
            return Generate(DefaultBatchCount);
        }

        public IReadOnlyList<string> Generate(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");

            var passphrases = new List<string>(count);
            for (var i = 0; i < count; i++)
                passphrases.Add(Next());

            return passphrases;
        }

        public Task<IReadOnlyList<string>> GenerateAsync(CancellationToken cancellationToken = default)
        {
            return GenerateAsync(DefaultBatchCount, cancellationToken);
        }

        public Task<IReadOnlyList<string>> GenerateAsync(int count, CancellationToken cancellationToken = default)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");

            var passphrases = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                passphrases.Add(Next());
            }

            return Task.FromResult<IReadOnlyList<string>>(passphrases);
        }

        /// <summary>Estimates passphrase entropy in bits from the word-list size, word count, and trailing number.</summary>
        public double EstimateEntropyBits()
        {
            var bits = WordCount * Math.Log(WordList.Words.Length, 2);
            if (IncludeNumber) bits += Math.Log(90, 2);
            return bits;
        }

        public void Dispose()
        {
            if (_ownsRandom && _random is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

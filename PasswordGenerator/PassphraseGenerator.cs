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

        /// <summary>Creates a passphrase generator.</summary>
        /// <param name="wordCount">The number of words in each passphrase; must be at least one.</param>
        /// <param name="separator">The character placed between words (and before the trailing number).</param>
        /// <param name="capitalize">Whether to capitalize the first letter of each word.</param>
        /// <param name="includeNumber">Whether to append a random two-digit number.</param>
        /// <param name="minimumEntropyBits">
        ///     An optional entropy floor. When greater than zero, the configuration is rejected if its
        ///     estimated entropy is below this many bits, so callers cannot silently produce weak phrases.
        /// </param>
        /// <param name="randomSource">
        ///     An optional random source. When supplied, the caller owns it; otherwise a
        ///     <see cref="CryptoRandomSource" /> is created and owned by this instance.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="wordCount" /> is less than one.</exception>
        /// <exception cref="ArgumentException">The estimated entropy is below <paramref name="minimumEntropyBits" />.</exception>
        public PassphraseGenerator(int wordCount = 4, char separator = '-', bool capitalize = false,
            bool includeNumber = true, double minimumEntropyBits = 0, IRandomSource? randomSource = null)
        {
            if (wordCount < 1)
                throw new ArgumentOutOfRangeException(nameof(wordCount), "A passphrase needs at least one word.");

            WordCount = wordCount;
            Separator = separator;
            Capitalize = capitalize;
            IncludeNumber = includeNumber;
            MinimumEntropyBits = minimumEntropyBits;
            _random = randomSource ?? new CryptoRandomSource();
            _ownsRandom = randomSource == null;

            if (minimumEntropyBits > 0 && EstimateEntropyBits() < minimumEntropyBits)
                throw new ArgumentException(
                    $"This passphrase yields about {EstimateEntropyBits():F1} bits of entropy, below the " +
                    $"required minimum of {minimumEntropyBits:F1} bits. Use more words " +
                    $"(at least {WordCountForEntropy(minimumEntropyBits, includeNumber)}).",
                    nameof(minimumEntropyBits));
        }

        /// <summary>The number of words in each passphrase.</summary>
        public int WordCount { get; }

        /// <summary>The character placed between words.</summary>
        public char Separator { get; }

        /// <summary>Whether the first letter of each word is capitalized.</summary>
        public bool Capitalize { get; }

        /// <summary>Whether a random two-digit number is appended.</summary>
        public bool IncludeNumber { get; }

        /// <summary>The entropy floor enforced at construction, in bits; zero means no floor.</summary>
        public double MinimumEntropyBits { get; }

        /// <summary>The number of passphrases produced by the parameterless <see cref="Generate()" /> overload.</summary>
        public int DefaultBatchCount { get; set; } = 1;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool TryNext(out string? password)
        {
            password = Next();
            return true;
        }

        /// <inheritdoc />
        public ValueTask<string> NextAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.IsCancellationRequested
                ? ValueTask.FromCanceled<string>(cancellationToken)
                : new ValueTask<string>(Next());
        }

        /// <inheritdoc />
        public IReadOnlyList<string> Generate()
        {
            return Generate(DefaultBatchCount);
        }

        /// <inheritdoc />
        public IReadOnlyList<string> Generate(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");

            var passphrases = new List<string>(count);
            for (var i = 0; i < count; i++)
                passphrases.Add(Next());

            return passphrases;
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyList<string>> GenerateAsync(CancellationToken cancellationToken = default)
        {
            return GenerateAsync(DefaultBatchCount, cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyList<string>> GenerateAsync(int count, CancellationToken cancellationToken = default)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");

            if (cancellationToken.IsCancellationRequested)
                return ValueTask.FromCanceled<IReadOnlyList<string>>(cancellationToken);

            var passphrases = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return ValueTask.FromCanceled<IReadOnlyList<string>>(cancellationToken);
                passphrases.Add(Next());
            }

            return new ValueTask<IReadOnlyList<string>>(passphrases);
        }

        /// <summary>Estimates passphrase entropy in bits from the word-list size, word count, and trailing number.</summary>
        public double EstimateEntropyBits()
        {
            var bits = WordCount * Math.Log(WordList.Words.Length, 2);
            if (IncludeNumber) bits += Math.Log(90, 2);
            return bits;
        }

        /// <summary>
        ///     Returns the smallest word count whose estimated entropy is at least
        ///     <paramref name="targetBits" />, accounting for the trailing number when requested.
        /// </summary>
        /// <param name="targetBits">The desired minimum entropy in bits.</param>
        /// <param name="includeNumber">Whether a trailing two-digit number will also be included.</param>
        public static int WordCountForEntropy(double targetBits, bool includeNumber = true)
        {
            var remaining = targetBits - (includeNumber ? Math.Log(90, 2) : 0);
            if (remaining <= 0) return 1;

            var bitsPerWord = Math.Log(WordList.Words.Length, 2);
            return Math.Max(1, (int)Math.Ceiling(remaining / bitsPerWord));
        }

        /// <summary>Disposes the random source when this instance owns it (i.e. it was not supplied by the caller).</summary>
        public void Dispose()
        {
            if (_ownsRandom && _random is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

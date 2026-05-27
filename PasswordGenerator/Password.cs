using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordGenerator
{
    /// <summary>
    ///     Generates random passwords that satisfy the configured rules.
    /// </summary>
    public class Password : IPassword, IPasswordGenerator, IDisposable
    {
        private const int DefaultPasswordLength = 16;
        private const int DefaultMaxPasswordAttempts = 10000;
        private const bool DefaultIncludeLowercase = true;
        private const bool DefaultIncludeUppercase = true;
        private const bool DefaultIncludeNumeric = true;
        private const bool DefaultIncludeSpecial = true;

        private readonly IRandomSource _random;
        private readonly bool _ownsRandom;

        /// <summary>Creates a generator with the default settings (all character classes, length 16).</summary>
        public Password()
        {
            Settings = new PasswordSettings(DefaultIncludeLowercase, DefaultIncludeUppercase,
                DefaultIncludeNumeric, DefaultIncludeSpecial, DefaultPasswordLength, DefaultMaxPasswordAttempts,
                true);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        /// <summary>Creates a generator from the supplied settings.</summary>
        /// <param name="settings">The settings to use.</param>
        public Password(IPasswordSettings settings)
        {
            Settings = settings;
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        /// <summary>Creates a generator with the default character classes and the given length.</summary>
        /// <param name="passwordLength">The required password length.</param>
        public Password(int passwordLength)
        {
            Settings = new PasswordSettings(DefaultIncludeLowercase, DefaultIncludeUppercase,
                DefaultIncludeNumeric, DefaultIncludeSpecial, passwordLength, DefaultMaxPasswordAttempts, true);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        /// <summary>Creates a generator with the given character classes enabled and the default length.</summary>
        /// <param name="includeLowercase">Whether to include lowercase letters.</param>
        /// <param name="includeUppercase">Whether to include uppercase letters.</param>
        /// <param name="includeNumeric">Whether to include digits.</param>
        /// <param name="includeSpecial">Whether to include special characters.</param>
        public Password(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial)
        {
            Settings = new PasswordSettings(includeLowercase, includeUppercase, includeNumeric,
                includeSpecial, DefaultPasswordLength, DefaultMaxPasswordAttempts, false);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        /// <summary>Creates a generator with the given character classes enabled and a specific length.</summary>
        /// <param name="includeLowercase">Whether to include lowercase letters.</param>
        /// <param name="includeUppercase">Whether to include uppercase letters.</param>
        /// <param name="includeNumeric">Whether to include digits.</param>
        /// <param name="includeSpecial">Whether to include special characters.</param>
        /// <param name="passwordLength">The required password length.</param>
        public Password(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength)
        {
            Settings = new PasswordSettings(includeLowercase, includeUppercase, includeNumeric,
                includeSpecial, passwordLength, DefaultMaxPasswordAttempts, false);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        /// <summary>Creates a generator with the given character classes, length, and attempt limit.</summary>
        /// <param name="includeLowercase">Whether to include lowercase letters.</param>
        /// <param name="includeUppercase">Whether to include uppercase letters.</param>
        /// <param name="includeNumeric">Whether to include digits.</param>
        /// <param name="includeSpecial">Whether to include special characters.</param>
        /// <param name="passwordLength">The required password length.</param>
        /// <param name="maximumAttempts">The maximum number of generation attempts.</param>
        public Password(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength, int maximumAttempts)
        {
            Settings = new PasswordSettings(includeLowercase, includeUppercase, includeNumeric,
                includeSpecial, passwordLength, maximumAttempts, false);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        /// <summary>
        ///     Creates a password generator with an explicit random source. The caller owns the
        ///     supplied <paramref name="randomSource" /> and is responsible for disposing it.
        /// </summary>
        public Password(IPasswordSettings settings, IRandomSource randomSource)
        {
            Settings = settings;
            _random = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
            _ownsRandom = false;
        }

        /// <summary>The settings that drive generation. Replaced in place by the fluent configuration methods.</summary>
        public IPasswordSettings Settings { get; set; }

        /// <inheritdoc />
        public IPassword IncludeLowercase()
        {
            Settings = Settings.AddLowercase();
            return this;
        }

        /// <inheritdoc />
        public IPassword IncludeUppercase()
        {
            Settings = Settings.AddUppercase();
            return this;
        }

        /// <inheritdoc />
        public IPassword IncludeNumeric()
        {
            Settings = Settings.AddNumeric();
            return this;
        }

        /// <inheritdoc />
        public IPassword IncludeSpecial()
        {
            Settings = Settings.AddSpecial();
            return this;
        }

        /// <inheritdoc />
        public IPassword IncludeSpecial(string specialCharactersToInclude)
        {
            Settings = Settings.AddSpecial(specialCharactersToInclude);
            return this;
        }

        /// <inheritdoc />
        public IPassword WithCharacters(string characters)
        {
            Settings = Settings.UseCharacters(characters);
            return this;
        }

        /// <inheritdoc />
        public IPassword WithAllAscii()
        {
            Settings = Settings.UseAllAscii();
            return this;
        }

        /// <inheritdoc />
        public IPassword ExcludeAmbiguous()
        {
            Settings = Settings.ExcludeAmbiguousCharacters();
            return this;
        }

        /// <inheritdoc />
        public IPassword RequireAtLeast(CharacterClass characterClass, int count)
        {
            Settings = Settings.RequireAtLeast(characterClass, count);
            return this;
        }

        /// <inheritdoc />
        public IPassword LengthRequired(int passwordLength)
        {
            Settings.PasswordLength = passwordLength;
            return this;
        }

        /// <summary>
        ///     The number of passwords produced by the parameterless <see cref="Generate()" /> overload.
        /// </summary>
        public int DefaultBatchCount { get; set; } = 1;

        /// <summary>Estimates the strength, in bits, of passwords produced from the current settings.</summary>
        public double EstimateEntropyBits()
        {
            return new PoolEntropyEstimator().EstimateBits(Settings);
        }

        /// <summary>
        ///     Generates a password that meets the configured requirements.
        /// </summary>
        /// <returns>A password as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when the configured settings cannot produce a valid password.</exception>
        public string Next()
        {
            if (!TryValidateSettings(out var error))
                throw new ArgumentException(error);

            return GenerateRandomPassword(Settings);
        }

        /// <summary>
        ///     Tries to generate a password. Returns false (instead of throwing) when the settings are invalid.
        /// </summary>
        public bool TryNext(out string? password)
        {
            if (!TryValidateSettings(out _))
            {
                password = null;
                return false;
            }

            password = GenerateRandomPassword(Settings);
            return true;
        }

        /// <inheritdoc />
        public IEnumerable<string> NextGroup(int numberOfPasswordsToGenerate)
        {
            return Generate(numberOfPasswordsToGenerate);
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

            var passwords = new List<string>(count);
            for (var i = 0; i < count; i++)
                passwords.Add(Next());

            return passwords;
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

            var passwords = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return ValueTask.FromCanceled<IReadOnlyList<string>>(cancellationToken);
                passwords.Add(Next());
            }

            return new ValueTask<IReadOnlyList<string>>(passwords);
        }

        private static readonly CharacterClass[] OrderedClasses =
        {
            CharacterClass.Lowercase, CharacterClass.Uppercase, CharacterClass.Numeric, CharacterClass.Special
        };

        private bool TryValidateSettings(out string? error)
        {
            if (!LengthIsValid(Settings.PasswordLength, Settings.MinimumLength, Settings.MaximumLength))
            {
                error =
                    $"Password length invalid. Must be between {Settings.MinimumLength} and {Settings.MaximumLength} characters long";
                return false;
            }

            if (Settings.IncludeSpecial && string.IsNullOrWhiteSpace(Settings.SpecialCharacters))
            {
                error = "Special characters are required but no special characters have been provided.";
                return false;
            }

            var pool = CharacterFilter.RemoveAmbiguous(Settings.CharacterSet, Settings.ExcludeAmbiguous);
            if (string.IsNullOrEmpty(pool))
            {
                error = "No characters are available to generate a password from.";
                return false;
            }

            if (!Settings.IsCustomPool)
            {
                var totalRequired = 0;
                foreach (var characterClass in OrderedClasses)
                {
                    var minimum = EffectiveMinimum(Settings, characterClass);
                    if (minimum <= 0) continue;

                    var group = CharacterFilter.RemoveAmbiguous(ClassCharacters(Settings, characterClass),
                        Settings.ExcludeAmbiguous);
                    if (group.Length == 0)
                    {
                        error =
                            $"At least {minimum} {characterClass} character(s) are required but none are available.";
                        return false;
                    }

                    totalRequired += minimum;
                }

                if (totalRequired > Settings.PasswordLength)
                {
                    error =
                        $"The required minimum characters ({totalRequired}) exceed the password length ({Settings.PasswordLength}).";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        ///     Builds a password that is valid by construction: the required minimum characters are taken
        ///     from each class first, the remainder is filled from the full pool, and the result is shuffled.
        /// </summary>
        private string GenerateRandomPassword(IPasswordSettings settings)
        {
            var length = settings.PasswordLength;
            var pool = CharacterFilter.RemoveAmbiguous(settings.CharacterSet, settings.ExcludeAmbiguous);

            var password = new char[length];
            var position = 0;

            if (!settings.IsCustomPool)
                foreach (var characterClass in OrderedClasses)
                {
                    var minimum = EffectiveMinimum(settings, characterClass);
                    if (minimum <= 0) continue;

                    var group = CharacterFilter.RemoveAmbiguous(ClassCharacters(settings, characterClass),
                        settings.ExcludeAmbiguous);

                    for (var k = 0; k < minimum && position < length; k++, position++)
                        password[position] = group[_random.NextInt(group.Length)];
                }

            for (; position < length; position++)
                password[position] = pool[_random.NextInt(pool.Length)];

            ShuffleInPlace(password);

            return new string(password);
        }

        private static int EffectiveMinimum(IPasswordSettings settings, CharacterClass characterClass)
        {
            if (!ClassEnabled(settings, characterClass)) return 0;
            // Each enabled class defaults to one guaranteed character unless overridden.
            return settings.MinimumCounts.TryGetValue(characterClass, out var minimum) ? minimum : 1;
        }

        private static bool ClassEnabled(IPasswordSettings settings, CharacterClass characterClass)
        {
            switch (characterClass)
            {
                case CharacterClass.Lowercase: return settings.IncludeLowercase;
                case CharacterClass.Uppercase: return settings.IncludeUppercase;
                case CharacterClass.Numeric: return settings.IncludeNumeric;
                case CharacterClass.Special: return settings.IncludeSpecial;
                default: return false;
            }
        }

        private static string ClassCharacters(IPasswordSettings settings, CharacterClass characterClass)
        {
            switch (characterClass)
            {
                case CharacterClass.Lowercase: return PasswordSettings.LowercaseCharacters;
                case CharacterClass.Uppercase: return PasswordSettings.UppercaseCharacters;
                case CharacterClass.Numeric: return PasswordSettings.NumericCharacters;
                case CharacterClass.Special: return settings.SpecialCharacters ?? string.Empty;
                default: return string.Empty;
            }
        }

        private void ShuffleInPlace(char[] items)
        {
            for (var i = items.Length - 1; i > 0; i--)
            {
                var j = _random.NextInt(i + 1);
                var temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }

        private static bool LengthIsValid(int passwordLength, int minLength, int maxLength)
        {
            return passwordLength >= minLength && passwordLength <= maxLength;
        }

        /// <summary>Disposes the random source when this instance owns it (i.e. it was not supplied by the caller).</summary>
        public void Dispose()
        {
            if (_ownsRandom && _random is IDisposable disposable)
                disposable.Dispose();
        }

        // ----- Presets (sugar over the fluent builder; later fluent calls still override) -----

        /// <summary>OWASP-style: the full printable-ASCII pool with no forced composition.</summary>
        public static IPassword ForOwasp(int length = 16)
        {
            return new Password().WithAllAscii().LengthRequired(length);
        }

        /// <summary>NIST 800-63B aligned: a long passphrase-friendly length over the full ASCII pool, no composition rules.</summary>
        public static IPassword ForNist(int length = 12)
        {
            return new Password().WithAllAscii().LengthRequired(length);
        }

        /// <summary>One-time-password style: a short numeric code.</summary>
        public static IPassword ForOtp(int digits = 6)
        {
            return new Password().WithCharacters(PasswordSettings.NumericCharacters).LengthRequired(digits);
        }

        /// <summary>API-key style: a long URL-safe secret.</summary>
        public static IPassword ForApiKey(int length = 32)
        {
            return new Password().WithCharacters(CharacterFilter.UrlSafeCharacters).LengthRequired(length);
        }

        /// <summary>Readable identifier: lowercase letters and digits with look-alikes removed.</summary>
        public static IPassword ForEnvironmentName(int length = 12)
        {
            return new Password()
                .WithCharacters(PasswordSettings.LowercaseCharacters + PasswordSettings.NumericCharacters)
                .ExcludeAmbiguous()
                .LengthRequired(length);
        }

        /// <summary>Diceware-style passphrase built from the EFF Large Wordlist.</summary>
        /// <param name="words">The number of words in the passphrase.</param>
        /// <param name="separator">The character placed between words, or <see langword="null" /> for no separator.</param>
        /// <param name="capitalize">Whether to capitalize the first letter of each word.</param>
        /// <param name="includeNumber">Whether to append a random two-digit number.</param>
        /// <param name="includeSymbol">Whether to attach a random symbol to one randomly chosen word.</param>
        /// <param name="minimumEntropyBits">
        ///     An optional entropy floor; when greater than zero the configuration is rejected if it
        ///     falls below this many bits.
        /// </param>
        public static IPasswordGenerator ForPassphrase(int words = 4, char? separator = '-',
            bool capitalize = false, bool includeNumber = true, bool includeSymbol = false,
            double minimumEntropyBits = 0)
        {
            return new PassphraseGenerator(words, separator, capitalize, includeNumber, includeSymbol,
                minimumEntropyBits);
        }

        /// <summary>
        ///     Diceware-style passphrase with at least <paramref name="targetBits" /> bits of entropy.
        ///     The word count is derived from the word-list size, and the same value is enforced as a floor.
        /// </summary>
        /// <param name="targetBits">The minimum entropy in bits (defaults to 80, a strong target).</param>
        /// <param name="separator">The character placed between words, or <see langword="null" /> for no separator.</param>
        /// <param name="capitalize">Whether to capitalize the first letter of each word.</param>
        /// <param name="includeNumber">Whether to append a random two-digit number.</param>
        /// <param name="includeSymbol">Whether to attach a random symbol to one randomly chosen word.</param>
        public static IPasswordGenerator ForPassphraseWithEntropy(double targetBits = 80, char? separator = '-',
            bool capitalize = false, bool includeNumber = true, bool includeSymbol = false)
        {
            var words = PassphraseGenerator.WordCountForEntropy(targetBits, includeNumber);
            return new PassphraseGenerator(words, separator, capitalize, includeNumber, includeSymbol, targetBits);
        }

        /// <summary>
        ///     A memorable, high-strength passphrase preset: capitalized words with a trailing number,
        ///     sized to at least 80 bits of entropy.
        /// </summary>
        public static IPasswordGenerator ForMemorable()
        {
            return ForPassphraseWithEntropy(80, separator: '-', capitalize: true, includeNumber: true);
        }
    }
}

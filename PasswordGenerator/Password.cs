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

        public Password()
        {
            Settings = new PasswordSettings(DefaultIncludeLowercase, DefaultIncludeUppercase,
                DefaultIncludeNumeric, DefaultIncludeSpecial, DefaultPasswordLength, DefaultMaxPasswordAttempts,
                true);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        public Password(IPasswordSettings settings)
        {
            Settings = settings;
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        public Password(int passwordLength)
        {
            Settings = new PasswordSettings(DefaultIncludeLowercase, DefaultIncludeUppercase,
                DefaultIncludeNumeric, DefaultIncludeSpecial, passwordLength, DefaultMaxPasswordAttempts, true);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        public Password(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial)
        {
            Settings = new PasswordSettings(includeLowercase, includeUppercase, includeNumeric,
                includeSpecial, DefaultPasswordLength, DefaultMaxPasswordAttempts, false);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

        public Password(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength)
        {
            Settings = new PasswordSettings(includeLowercase, includeUppercase, includeNumeric,
                includeSpecial, passwordLength, DefaultMaxPasswordAttempts, false);
            _random = new CryptoRandomSource();
            _ownsRandom = true;
        }

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

        public IPasswordSettings Settings { get; set; }

        public IPassword IncludeLowercase()
        {
            Settings = Settings.AddLowercase();
            return this;
        }

        public IPassword IncludeUppercase()
        {
            Settings = Settings.AddUppercase();
            return this;
        }

        public IPassword IncludeNumeric()
        {
            Settings = Settings.AddNumeric();
            return this;
        }

        public IPassword IncludeSpecial()
        {
            Settings = Settings.AddSpecial();
            return this;
        }

        public IPassword IncludeSpecial(string specialCharactersToInclude)
        {
            Settings = Settings.AddSpecial(specialCharactersToInclude);
            return this;
        }

        public IPassword LengthRequired(int passwordLength)
        {
            Settings.PasswordLength = passwordLength;
            return this;
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

        public IEnumerable<string> NextGroup(int numberOfPasswordsToGenerate)
        {
            return Generate(numberOfPasswordsToGenerate);
        }

        public Task<string> NextAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Next());
        }

        public IReadOnlyList<string> Generate(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");

            var passwords = new List<string>(count);
            for (var i = 0; i < count; i++)
                passwords.Add(Next());

            return passwords;
        }

        public Task<IReadOnlyList<string>> GenerateAsync(int count, CancellationToken cancellationToken = default)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");

            var passwords = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                passwords.Add(Next());
            }

            return Task.FromResult<IReadOnlyList<string>>(passwords);
        }

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

            if (string.IsNullOrEmpty(Settings.CharacterSet))
            {
                error = "No character sets have been selected to generate a password from.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        ///     Builds a password that is valid by construction: one character is taken from each required
        ///     class first, the remainder is filled from the full pool, and the result is shuffled.
        /// </summary>
        private string GenerateRandomPassword(IPasswordSettings settings)
        {
            var length = settings.PasswordLength;
            var pool = settings.CharacterSet;
            var groups = settings.CharacterGroups;

            var password = new char[length];
            var position = 0;

            // Guarantee at least one character from each required class (only as many as fit).
            for (var i = 0; i < groups.Count && position < length; i++, position++)
            {
                var group = groups[i];
                password[position] = group[_random.NextInt(group.Length)];
            }

            // Fill the rest from the full character pool.
            for (; position < length; position++)
                password[position] = pool[_random.NextInt(pool.Length)];

            ShuffleInPlace(password);

            return new string(password);
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

        public void Dispose()
        {
            if (_ownsRandom && _random is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

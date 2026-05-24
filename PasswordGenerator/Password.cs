using System;
using System.Collections.Generic;

namespace PasswordGenerator
{
/// <summary>
    ///     Generates random passwords and validates that they meet the rules passed in
    /// </summary>
    public class Password : IPassword
    {
        private const int DefaultPasswordLength = 16;
        private const int DefaultMaxPasswordAttempts = 10000;
        private const bool DefaultIncludeLowercase = true;
        private const bool DefaultIncludeUppercase = true;
        private const bool DefaultIncludeNumeric = true;
        private const bool DefaultIncludeSpecial = true;

        private readonly IRandomSource _randomSource;

        public Password()
            : this(new PasswordSettings(DefaultIncludeLowercase, DefaultIncludeUppercase,
                DefaultIncludeNumeric, DefaultIncludeSpecial, DefaultPasswordLength, DefaultMaxPasswordAttempts,
                true))
        {
        }

        public Password(IPasswordSettings settings)
            : this(settings, new CryptoRandomSource())
        {
        }

        public Password(IPasswordSettings settings, IRandomSource randomSource)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        }

        public Password(int passwordLength)
            : this(new PasswordSettings(DefaultIncludeLowercase, DefaultIncludeUppercase,
                DefaultIncludeNumeric, DefaultIncludeSpecial, passwordLength, DefaultMaxPasswordAttempts, true))
        {
        }

        public Password(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial)
            : this(new PasswordSettings(includeLowercase, includeUppercase, includeNumeric,
                includeSpecial, DefaultPasswordLength, DefaultMaxPasswordAttempts, false))
        {
        }

        public Password(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength)
            : this(new PasswordSettings(includeLowercase, includeUppercase, includeNumeric,
                includeSpecial, passwordLength, DefaultMaxPasswordAttempts, false))
        {
        }

        public Password(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength, int maximumAttempts)
            : this(new PasswordSettings(includeLowercase, includeUppercase, includeNumeric,
                includeSpecial, passwordLength, maximumAttempts, false))
        {
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
        ///     Gets the next random password which meets the requirements.
        /// </summary>
        /// <returns>A password as a string.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when the configured settings cannot produce a valid password (e.g. no character
        ///     class included, an empty custom special set, or an out-of-range length). The generator
        ///     never returns an error message in place of a password.
        /// </exception>
        public string Next()
        {
            ValidateSettings(Settings);
            return GenerateRandomPassword(Settings);
        }

        /// <summary>
        ///     Attempts to generate a password. Returns <c>false</c> (and a <c>null</c> password)
        ///     instead of throwing when the settings are invalid.
        /// </summary>
        public bool TryNext(out string password)
        {
            try
            {
                password = Next();
                return true;
            }
            catch (ArgumentException)
            {
                password = null;
                return false;
            }
        }

        public IEnumerable<string> NextGroup(int numberOfPasswordsToGenerate)
        {
            var passwords = new List<string>();

            for (var i = 0; i < numberOfPasswordsToGenerate; i++)
            {
                var pwd = this.Next();
                passwords.Add(pwd);
            }

            return passwords;
        }

        /// <summary>
        ///     Generates a random password that is valid by construction: one character is placed from
        ///     every included character class, the rest are drawn from the combined pool, and the whole
        ///     buffer is shuffled with the cryptographic source. No validate-and-retry loop is needed.
        /// </summary>
        private string GenerateRandomPassword(IPasswordSettings settings)
        {
            var groups = settings.CharacterGroups;
            var pool = string.Concat(groups);
            var length = settings.PasswordLength;
            var password = new char[length];

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                password[i] = group[_randomSource.NextInt(group.Length)];
            }

            for (var i = groups.Count; i < length; i++)
                password[i] = pool[_randomSource.NextInt(pool.Length)];

            Shuffle(password);

            return new string(password);
        }

        /// <summary>
        ///     Validates the configuration up front and throws a clear exception rather than emitting
        ///     a magic error string or silently failing.
        /// </summary>
        private static void ValidateSettings(IPasswordSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.IncludeSpecial && string.IsNullOrWhiteSpace(settings.SpecialCharacters))
                throw new ArgumentException(
                    "IncludeSpecial was requested but the special character set is empty or whitespace.");

            var groups = settings.CharacterGroups;
            if (groups.Count == 0)
                throw new ArgumentException("At least one character class must be included.");

            if (!LengthIsValid(settings.PasswordLength, settings.MinimumLength, settings.MaximumLength))
                throw new ArgumentException(
                    $"Password length invalid. Must be between {settings.MinimumLength} and {settings.MaximumLength} characters long");

            if (settings.PasswordLength < groups.Count)
                throw new ArgumentException(
                    $"Password length {settings.PasswordLength} is too short to include one character from each of the {groups.Count} required character classes.");
        }

        /// <summary>
        ///     Checks that the password is within the valid length range
        /// </summary>
        private static bool LengthIsValid(int passwordLength, int minLength, int maxLength)
        {
            return passwordLength >= minLength && passwordLength <= maxLength;
        }

        private void Shuffle(char[] items)
        {
            for (var i = items.Length - 1; i > 0; i--)
            {
                var j = _randomSource.NextInt(i + 1);
                var temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }
    }
}

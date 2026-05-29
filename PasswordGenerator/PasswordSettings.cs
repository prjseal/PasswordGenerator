using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordGenerator
{
    /// <summary>
    ///     Holds all of the settings for the password generator
    /// </summary>
    public class PasswordSettings : IPasswordSettings
    {
        /// <summary>The lowercase letters (<c>a–z</c>) used when lowercase is enabled.</summary>
        public const string LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>The uppercase letters (<c>A–Z</c>) used when uppercase is enabled.</summary>
        public const string UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>The digits (<c>0–9</c>) used when numeric is enabled.</summary>
        public const string NumericCharacters = "0123456789";

        private const string DefaultSpecialCharacters = @"!#$%&*@\";
        private const int DefaultMinPasswordLength = 4;
        private const int DefaultMaxPasswordLength = 256;

        /// <inheritdoc />
        public string SpecialCharacters { get; set; }

        /// <summary>Creates a settings instance.</summary>
        /// <param name="includeLowercase">Whether to include lowercase letters.</param>
        /// <param name="includeUppercase">Whether to include uppercase letters.</param>
        /// <param name="includeNumeric">Whether to include digits.</param>
        /// <param name="includeSpecial">Whether to include special characters.</param>
        /// <param name="passwordLength">The required password length.</param>
        /// <param name="maximumAttempts">The maximum number of generation attempts.</param>
        /// <param name="usingDefaults">
        ///     Whether these are the library defaults; when <see langword="true" />, the first fluent
        ///     configuration call clears the default pool before applying changes.
        /// </param>
        public PasswordSettings(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength, int maximumAttempts, bool usingDefaults)
        {
            IncludeLowercase = includeLowercase;
            IncludeUppercase = includeUppercase;
            IncludeNumeric = includeNumeric;
            IncludeSpecial = includeSpecial;
            PasswordLength = passwordLength;
            MaximumAttempts = maximumAttempts;
            MinimumLength = DefaultMinPasswordLength;
            MaximumLength = DefaultMaxPasswordLength;
            UsingDefaults = usingDefaults;
            SpecialCharacters = DefaultSpecialCharacters;
            CharacterSet = BuildCharacterSet(includeLowercase, includeUppercase, includeNumeric, includeSpecial);
        }

        private bool UsingDefaults { get; set; }
        private readonly Dictionary<CharacterClass, int> _minimumCounts = new Dictionary<CharacterClass, int>();

        /// <inheritdoc />
        public bool IncludeLowercase { get; private set; }

        /// <inheritdoc />
        public bool IncludeUppercase { get; private set; }

        /// <inheritdoc />
        public bool IncludeNumeric { get; private set; }

        /// <inheritdoc />
        public bool IncludeSpecial { get; private set; }

        /// <inheritdoc />
        public int PasswordLength { get; set; }

        /// <inheritdoc />
        public string CharacterSet { get; private set; }

        /// <inheritdoc />
        public bool IsCustomPool { get; private set; }

        /// <inheritdoc />
        public bool ExcludeAmbiguous { get; private set; }

        /// <inheritdoc />
        public IReadOnlyDictionary<CharacterClass, int> MinimumCounts => _minimumCounts;

        /// <inheritdoc />
        public int MaximumAttempts { get; }

        /// <inheritdoc />
        public int MinimumLength { get; }

        /// <inheritdoc />
        public int MaximumLength { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> CharacterGroups
        {
            get
            {
                var groups = new List<string>();
                if (IncludeLowercase) groups.Add(LowercaseCharacters);
                if (IncludeUppercase) groups.Add(UppercaseCharacters);
                if (IncludeNumeric) groups.Add(NumericCharacters);
                if (IncludeSpecial && !string.IsNullOrEmpty(SpecialCharacters)) groups.Add(SpecialCharacters);
                return groups;
            }
        }

        /// <inheritdoc />
        public IPasswordSettings AddLowercase()
        {
            StopUsingDefaults();
            IncludeLowercase = true;
            CharacterSet += LowercaseCharacters;
            return this;
        }

        /// <inheritdoc />
        public IPasswordSettings AddUppercase()
        {
            StopUsingDefaults();
            IncludeUppercase = true;
            CharacterSet += UppercaseCharacters;
            return this;
        }

        /// <inheritdoc />
        public IPasswordSettings AddNumeric()
        {
            StopUsingDefaults();
            IncludeNumeric = true;
            CharacterSet += NumericCharacters;
            return this;
        }

        /// <inheritdoc />
        public IPasswordSettings AddSpecial()
        {
            StopUsingDefaults();
            IncludeSpecial = true;
            SpecialCharacters = DefaultSpecialCharacters;
            CharacterSet += SpecialCharacters;
            return this;
        }

        /// <inheritdoc />
        public IPasswordSettings AddSpecial(string specialCharactersToAdd)
        {
            StopUsingDefaults();
            IncludeSpecial = true;
            SpecialCharacters = specialCharactersToAdd;
            CharacterSet += specialCharactersToAdd;
            return this;
        }

        /// <inheritdoc />
        public IPasswordSettings UseCharacters(string characters)
        {
            if (characters == null) throw new ArgumentNullException(nameof(characters));

            StopUsingDefaults();
            IncludeLowercase = false;
            IncludeUppercase = false;
            IncludeNumeric = false;
            IncludeSpecial = false;
            _minimumCounts.Clear();
            IsCustomPool = true;
            CharacterSet = characters;
            return this;
        }

        /// <inheritdoc />
        public IPasswordSettings UseAllAscii()
        {
            return UseCharacters(CharacterFilter.AllPrintableAscii);
        }

        /// <inheritdoc />
        public IPasswordSettings ExcludeAmbiguousCharacters()
        {
            ExcludeAmbiguous = true;
            return this;
        }

        /// <inheritdoc />
        public IPasswordSettings RequireAtLeast(CharacterClass characterClass, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");

            if (IsCustomPool)
                throw new InvalidOperationException(
                    "Per-class minimums cannot be combined with a custom character pool.");

            // Requiring a class implies it is part of the pool.
            if (count > 0)
                switch (characterClass)
                {
                    case CharacterClass.Lowercase:
                        if (!IncludeLowercase) AddLowercase();
                        break;
                    case CharacterClass.Uppercase:
                        if (!IncludeUppercase) AddUppercase();
                        break;
                    case CharacterClass.Numeric:
                        if (!IncludeNumeric) AddNumeric();
                        break;
                    case CharacterClass.Special:
                        if (!IncludeSpecial) AddSpecial();
                        break;
                }

            _minimumCounts[characterClass] = count;
            return this;
        }

        private string BuildCharacterSet(bool includeLowercase, bool includeUppercase, bool includeNumeric,
            bool includeSpecial)
        {
            var characterSet = new StringBuilder();
            if (includeLowercase) characterSet.Append(LowercaseCharacters);

            if (includeUppercase) characterSet.Append(UppercaseCharacters);

            if (includeNumeric) characterSet.Append(NumericCharacters);

            if (includeSpecial) characterSet.Append(SpecialCharacters);
            return characterSet.ToString();
        }

        private void StopUsingDefaults()
        {
            if (!UsingDefaults) return;
            CharacterSet = string.Empty;
            IncludeLowercase = false;
            IncludeUppercase = false;
            IncludeNumeric = false;
            IncludeSpecial = false;
            UsingDefaults = false;
        }
    }
}
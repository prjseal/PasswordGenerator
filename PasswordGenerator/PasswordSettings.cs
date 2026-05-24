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
        public const string LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
        public const string UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string NumericCharacters = "0123456789";
        private const string DefaultSpecialCharacters = @"!#$%&*@\";
        private const int DefaultMinPasswordLength = 4;
        private const int DefaultMaxPasswordLength = 256;
        public string SpecialCharacters { get; set; }

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

        public bool IncludeLowercase { get; private set; }
        public bool IncludeUppercase { get; private set; }
        public bool IncludeNumeric { get; private set; }
        public bool IncludeSpecial { get; private set; }
        public int PasswordLength { get; set; }
        public string CharacterSet { get; private set; }
        public bool IsCustomPool { get; private set; }
        public bool ExcludeAmbiguous { get; private set; }
        public IReadOnlyDictionary<CharacterClass, int> MinimumCounts => _minimumCounts;
        public int MaximumAttempts { get; }
        public int MinimumLength { get; }
        public int MaximumLength { get; }

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

        public IPasswordSettings AddLowercase()
        {
            StopUsingDefaults();
            IncludeLowercase = true;
            CharacterSet += LowercaseCharacters;
            return this;
        }

        public IPasswordSettings AddUppercase()
        {
            StopUsingDefaults();
            IncludeUppercase = true;
            CharacterSet += UppercaseCharacters;
            return this;
        }

        public IPasswordSettings AddNumeric()
        {
            StopUsingDefaults();
            IncludeNumeric = true;
            CharacterSet += NumericCharacters;
            return this;
        }

        public IPasswordSettings AddSpecial()
        {
            StopUsingDefaults();
            IncludeSpecial = true;
            SpecialCharacters = DefaultSpecialCharacters;
            CharacterSet += SpecialCharacters;
            return this;
        }

        public IPasswordSettings AddSpecial(string specialCharactersToAdd)
        {
            StopUsingDefaults();
            IncludeSpecial = true;
            SpecialCharacters = specialCharactersToAdd;
            CharacterSet += specialCharactersToAdd;
            return this;
        }

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

        public IPasswordSettings UseAllAscii()
        {
            return UseCharacters(CharacterFilter.AllPrintableAscii);
        }

        public IPasswordSettings ExcludeAmbiguousCharacters()
        {
            ExcludeAmbiguous = true;
            return this;
        }

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
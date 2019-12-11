using System.Text;

namespace PasswordGenerator
{
    /// <summary>
    ///     Holds all of the settings for the password generator
    /// </summary>
    public class PasswordSettings : IPasswordSettings
    {
        private const string LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumericCharacters = "0123456789";
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

        public bool IncludeLowercase { get; private set; }
        public bool IncludeUppercase { get; private set; }
        public bool IncludeNumeric { get; private set; }
        public bool IncludeSpecial { get; private set; }
        public int PasswordLength { get; set; }
        public string CharacterSet { get; private set; }
        public int MaximumAttempts { get; }
        public int MinimumLength { get; }
        public int MaximumLength { get; }

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
using System;

namespace PasswordGenerator
{
    [Obsolete("The class 'PasswordGenerator' is obsolete. Use 'Password' instead.")]
    public class Generator : Password
    {
        public Generator()
        {

        }

        public Generator(IPasswordSettings settings) : base(settings)
        {
        }

        public Generator(int passwordLength) : base(passwordLength)
        {
        }

        public Generator(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial) : base(includeLowercase, includeUppercase, includeNumeric, includeSpecial)
        {
        }

        public Generator(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength) : base(includeLowercase, includeUppercase, includeNumeric, includeSpecial, passwordLength)
        {
        }

        public Generator(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength, int maximumAttempts) : base(includeLowercase, includeUppercase, includeNumeric, includeSpecial,
            passwordLength, maximumAttempts)
        {
        }

        public new Generator IncludeLowercase()
        {
            Settings = Settings.AddLowercase();
            return this;
        }

        public new Generator IncludeUppercase()
        {
            Settings = Settings.AddUppercase();
            return this;
        }

        public new Generator IncludeNumeric()
        {
            Settings = Settings.AddNumeric();
            return this;
        }

        public new Generator IncludeSpecial()
        {
            Settings = Settings.AddSpecial();
            return this;
        }

        public new Generator LengthRequired(int passwordLength)
        {
            Settings.Length = passwordLength;
            return this;
        }
    }
}

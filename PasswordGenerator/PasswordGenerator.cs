using System;

namespace PasswordGenerator
{
    [ObsoleteAttribute("The class 'PasswordGenerator' is obsolete. Use 'Password' instead.")]
    public class PasswordGenerator : Password
    {
        public PasswordGenerator()
        {

        }

        public PasswordGenerator(PasswordGeneratorSettings settings) : base (settings)
        {
        }

        public PasswordGenerator(int passwordLength) : base(passwordLength)
        {
        }

        public PasswordGenerator(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial) : base(includeLowercase, includeUppercase, includeNumeric, includeSpecial)
        {
        }

        public PasswordGenerator(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength) : base(includeLowercase, includeUppercase, includeNumeric, includeSpecial, passwordLength)
        {
        }

        public PasswordGenerator(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial,
            int passwordLength, int maximumAttempts) : base(includeLowercase, includeUppercase, includeNumeric, includeSpecial,
            passwordLength, maximumAttempts)
        {
        }

        public PasswordGenerator IncludeLowercase()
        {
            base.Settings = base.Settings.AddLowercase();
            return this;
        }

        public PasswordGenerator IncludeUppercase()
        {
            base.Settings = base.Settings.AddUppercase();
            return this;
        }

        public PasswordGenerator IncludeNumeric()
        {
            base.Settings = base.Settings.AddNumeric();
            return this;
        }

        public PasswordGenerator IncludeSpecial()
        {
            base.Settings = base.Settings.AddSpecial();
            return this;
        }

        public PasswordGenerator LengthRequired(int passwordLength)
        {
            base.Settings.PasswordLength = passwordLength;
            return this;
        }
    }
}

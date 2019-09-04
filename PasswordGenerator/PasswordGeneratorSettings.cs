namespace PasswordGenerator
{
    public class PasswordGeneratorSettings : PasswordSettings
    {
        public PasswordGeneratorSettings(bool includeLowercase, bool includeUppercase, bool includeNumeric, 
            bool includeSpecial, int passwordLength, int maximumAttempts, bool usingDefaults) 
            : base(includeLowercase, includeUppercase, includeNumeric, 
                includeSpecial, passwordLength, maximumAttempts, usingDefaults)
        {
        }
    }
}

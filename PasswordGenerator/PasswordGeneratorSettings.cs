namespace PasswordGenerator
{
    public class GeneratorSettings : Settings
    {
        public GeneratorSettings(bool includeLowercase, bool includeUppercase, bool includeNumeric, 
            bool includeSpecial, int passwordLength, int maximumAttempts, bool usingDefaults) 
            : base(includeLowercase, includeUppercase, includeNumeric, 
                includeSpecial, passwordLength, maximumAttempts, usingDefaults)
        {
        }
    }
}

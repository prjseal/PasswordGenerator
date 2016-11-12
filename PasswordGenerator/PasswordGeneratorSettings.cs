using System.Text;
using System.Configuration;

/// <summary>
/// Holds all of the settings for the password generator
/// </summary>
public class PasswordGeneratorSettings
{
    const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
    const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string NUMERIC_CHARACTERS = "0123456789";
    const string SPECIAL_CHARACTERS = @"!#$%&*@\";
    int _defaultPasswordLength = 8; // int.Parse(ConfigurationManager.AppSettings["DefaultPasswordLength"]);
    int _defaultMinPasswordLength = 8; //int.Parse(ConfigurationManager.AppSettings["MinPasswordLength"]);
    int _defaultMaxPasswordLength = 128;//int.Parse(ConfigurationManager.AppSettings["MaxPasswordLength"]);
    int _defaultMaxPasswordAttempts = 10000; //int.Parse(ConfigurationManager.AppSettings["InvalidPasswordLengthMessage"]);

    public bool IncludeLowercase { get; set; }
    public bool IncludeUppercase { get; set; }
    public bool IncludeNumeric { get; set; }
    public bool IncludeSpecial { get; set; }
    public int PasswordLength { get; set; }
    public string CharacterSet { get; set; }
    public int MaximumAttempts { get; set; }
    public int MinimumLength { get; set; }
    public int MaximumLength { get; set; }

    public PasswordGeneratorSettings(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial)
    {
        SetPasswordGeneratorProperties(includeLowercase, includeUppercase, includeNumeric, includeSpecial, _defaultPasswordLength, _defaultMaxPasswordAttempts);
    }

    public PasswordGeneratorSettings(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, int passwordLength)
    {
        SetPasswordGeneratorProperties(includeLowercase, includeUppercase, includeNumeric, includeSpecial, passwordLength, _defaultMaxPasswordAttempts);
    }

    public PasswordGeneratorSettings(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, int passwordLength, int maximumAttempts)
    {
        SetPasswordGeneratorProperties(includeLowercase, includeUppercase, includeNumeric, includeSpecial, passwordLength, maximumAttempts);
    }

    private void SetPasswordGeneratorProperties(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, int passwordLength, int maximumAttempts)
    {
        IncludeLowercase = includeLowercase;
        IncludeUppercase = includeUppercase;
        IncludeNumeric = includeNumeric;
        IncludeSpecial = includeSpecial;
        PasswordLength = passwordLength;
        MaximumAttempts = maximumAttempts;
        MinimumLength = _defaultMinPasswordLength;
        MaximumLength = _defaultMaxPasswordLength;
        StringBuilder characterSet = new StringBuilder();

        AppendCharactersIfTrue(includeLowercase, LOWERCASE_CHARACTERS, ref characterSet);
        AppendCharactersIfTrue(includeUppercase, UPPERCASE_CHARACTERS, ref characterSet);
        AppendCharactersIfTrue(includeNumeric, NUMERIC_CHARACTERS, ref characterSet);
        AppendCharactersIfTrue(includeSpecial, SPECIAL_CHARACTERS, ref characterSet);

        CharacterSet = characterSet.ToString();
    }

    private void AppendCharactersIfTrue(bool includeCharacters, string characters, ref StringBuilder characterSet)
    {
        if (includeCharacters)
        {
            characterSet.Append(LOWERCASE_CHARACTERS);
        }
    }

}
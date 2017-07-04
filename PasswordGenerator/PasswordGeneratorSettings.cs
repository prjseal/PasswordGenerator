using System.Text;

/// <summary>
/// Holds all of the settings for the password generator
/// </summary>
public class PasswordGeneratorSettings
{
    const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
    const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string NUMERIC_CHARACTERS = "0123456789";
    const string SPECIAL_CHARACTERS = @"!#$%&*@\";
    int _defaultMinPasswordLength = 8;
    int _defaultMaxPasswordLength = 128;

    public bool IncludeLowercase { get; set; }
    public bool IncludeUppercase { get; set; }
    public bool IncludeNumeric { get; set; }
    public bool IncludeSpecial { get; set; }
    public int PasswordLength { get; set; }
    public string CharacterSet { get; set; }
    public int MaximumAttempts { get; set; }
    public int MinimumLength { get; set; }
    public int MaximumLength { get; set; }
    public bool UsingDefaults { get; set; }

    public PasswordGeneratorSettings(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, int passwordLength, int maximumAttempts, bool usingDefaults)
    {
        IncludeLowercase = includeLowercase;
        IncludeUppercase = includeUppercase;
        IncludeNumeric = includeNumeric;
        IncludeSpecial = includeSpecial;
        PasswordLength = passwordLength;
        MaximumAttempts = maximumAttempts;
        MinimumLength = _defaultMinPasswordLength;
        MaximumLength = _defaultMaxPasswordLength;
        UsingDefaults = usingDefaults;
        CharacterSet = BuildCharacterSet(includeLowercase, includeUppercase, includeNumeric, includeSpecial);
    }

    private string BuildCharacterSet(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial)
    {
        StringBuilder characterSet = new StringBuilder();
        if (includeLowercase)
        {
            characterSet.Append(LOWERCASE_CHARACTERS);
        }

        if (includeUppercase)
        {
            characterSet.Append(UPPERCASE_CHARACTERS);
        }

        if (includeNumeric)
        {
            characterSet.Append(NUMERIC_CHARACTERS);
        }

        if (includeSpecial)
        {
            characterSet.Append(SPECIAL_CHARACTERS);
        }
        return characterSet.ToString();
    }
    
    public PasswordGeneratorSettings AddLowercase()
    {
        StopUsingDefaults();
        IncludeLowercase = true;
        CharacterSet += LOWERCASE_CHARACTERS;
        return this;
    }

    public PasswordGeneratorSettings AddUppercase()
    {
        StopUsingDefaults();
        IncludeUppercase = true;
        CharacterSet += UPPERCASE_CHARACTERS;
        return this;
    }

    public PasswordGeneratorSettings AddNumeric()
    {
        StopUsingDefaults();
        IncludeNumeric = true;
        CharacterSet += NUMERIC_CHARACTERS;
        return this;
    }

    public PasswordGeneratorSettings AddSpecial()
    {
        StopUsingDefaults();
        IncludeSpecial = true;
        CharacterSet += SPECIAL_CHARACTERS;
        return this;
    }

    private void StopUsingDefaults()
    {
        if (UsingDefaults)
        {
            CharacterSet = string.Empty;
            IncludeLowercase = false;
            IncludeUppercase = false;
            IncludeNumeric = false;
            IncludeSpecial = false;
            UsingDefaults = false;
        }
    }

}
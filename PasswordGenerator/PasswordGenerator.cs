using System.Text.RegularExpressions;
using System.Configuration;

/// <summary>
/// Generates random passwords and validates that they meet the rules passed in
/// </summary>
public static class PasswordGenerator
{
    /// <summary>
    /// Calls the method to generate a random password and continues until it returns
    /// a valid password or the number of attempts exceeds the limit.
    /// </summary>
    /// <param name="settings">Password generator settings object</param>
    /// <returns>A random password which is validated</returns>
    public static string GetValidPassword(PasswordGeneratorSettings settings)
    {
        string password;
        if (!LengthIsValid(settings.PasswordLength, settings.MinimumLength, settings.MaximumLength))
        {
            password = string.Format("Password length invalid. Must be between {0} and {1} characters long", settings.MinimumLength, settings.MaximumLength);
        }
        else
        {
            int passwordAttempts = 0;
            do
            {
                password = PasswordGenerator.GenerateRandomPassword(settings);
                passwordAttempts++;
            }
            while (passwordAttempts < settings.MaximumAttempts && !PasswordGenerator.PasswordIsValid(settings, password));

            password = PasswordGenerator.PasswordIsValid(settings, password) ? password : "Try again";
        }

        return password;
    }


    /// <summary>
    /// Generates a random password based on the rules passed in the settings parameter
    /// This does not do any validation
    /// </summary>
    /// <param name="settings">Password generator settings object</param>
    /// <returns>a random password</returns>
    public static string GenerateRandomPassword(PasswordGeneratorSettings settings)
    {
        const int MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS = 2;
        char[] password = new char[settings.PasswordLength];
        int characterSetLength = settings.CharacterSet.Length;

        System.Random random = new System.Random();
        for (int characterPosition = 0; characterPosition < settings.PasswordLength; characterPosition++)
        {
            password[characterPosition] = settings.CharacterSet[random.Next(characterSetLength - 1)];

            bool moreThanTwoIdenticalInARow =
                characterPosition > MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS
                && password[characterPosition] == password[characterPosition - 1]
                && password[characterPosition - 1] == password[characterPosition - 2];

            if (moreThanTwoIdenticalInARow)
            {
                characterPosition--;
            }
        }

        return string.Join(null, password);
    }

    /// <summary>
    /// When you give it a password and some settings, it validates the password against the settings.
    /// </summary>
    /// <param name="settings">Password settings</param>
    /// <param name="password">Password to test</param>
    /// <returns>True or False to say if the password is valid or not</returns>
    public static bool PasswordIsValid(PasswordGeneratorSettings settings, string password)
    {
        const string REGEX_LOWERCASE = @"[a-z]";
        const string REGEX_UPPERCASE = @"[A-Z]";
        const string REGEX_NUMERIC = @"[\d]";
        const string REGEX_SPECIAL = @"([!#$%&*@\\])+";
        const int PASSWORD_LENGTH_MIN = 8;
        const int PASSWORD_LENGTH_MAX = 128;

        bool lowerCaseIsValid = ValidateCharacterSet(settings.IncludeLowercase, password, REGEX_LOWERCASE);
        bool upperCaseIsValid = ValidateCharacterSet(settings.IncludeUppercase, password, REGEX_UPPERCASE);
        bool numericIsValid = ValidateCharacterSet(settings.IncludeNumeric, password, REGEX_NUMERIC);
        bool symbolsAreValid = !settings.IncludeSpecial || (settings.IncludeSpecial && Regex.IsMatch(password, REGEX_SPECIAL));
        bool isValidLength = LengthIsValid(password.Length, PASSWORD_LENGTH_MIN, PASSWORD_LENGTH_MAX);

        return lowerCaseIsValid && upperCaseIsValid && numericIsValid && symbolsAreValid;
    }

    private static bool ValidateCharacterSet(bool includeThisCharacterSet, string password, string charactersetRegex)
    {
        return !includeThisCharacterSet || (includeThisCharacterSet && Regex.IsMatch(password, charactersetRegex));
    }

    /// <summary>
    /// Checks that the password is within the valid length range
    /// </summary>
    /// <param name="password">The password to validate</param>
    /// <returns>A bool to say if it is valid or not</returns>
    public static bool LengthIsValid(int passwordLength, int minLength, int maxLength)
    {
        return passwordLength >= minLength && passwordLength <= maxLength;
    }
}

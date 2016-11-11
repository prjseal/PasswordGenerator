using System.Text.RegularExpressions;

/// <summary>
/// Generates random passwords and validates that they meet the rules passed in
/// </summary>
public static class PasswordGenerator
{
    /// <summary>
    /// Generates a random password based on the rules passed in the settings parameter
    /// </summary>
    /// <param name="settings">Password generator settings object</param>
    /// <returns>Password or try again</returns>
    public static string GeneratePassword(PasswordGeneratorSettings settings)
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

        bool lowerCaseIsValid = !settings.IncludeLowercase || (settings.IncludeLowercase && Regex.IsMatch(password, REGEX_LOWERCASE));
        bool upperCaseIsValid = !settings.IncludeUppercase || (settings.IncludeUppercase && Regex.IsMatch(password, REGEX_UPPERCASE));
        bool numericIsValid = !settings.IncludeNumbers || (settings.IncludeNumbers && Regex.IsMatch(password, REGEX_NUMERIC));
        bool symbolsAreValid = !settings.IncludeSpecial || (settings.IncludeSpecial && Regex.IsMatch(password, REGEX_SPECIAL));

        return lowerCaseIsValid && upperCaseIsValid && numericIsValid && symbolsAreValid;
    }
}

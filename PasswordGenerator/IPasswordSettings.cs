using System.Collections.Generic;

namespace PasswordGenerator
{
    /// <summary>
    ///     Holds all of the settings for the password generator
    /// </summary>
    public interface IPasswordSettings
    {
        bool IncludeLowercase { get; }
        bool IncludeUppercase { get; }
        bool IncludeNumeric { get; }
        bool IncludeSpecial { get; }
        int PasswordLength { get; set; }
        string CharacterSet { get; }

        /// <summary>
        ///     The individual character groups that are included (one per enabled class). Used to
        ///     guarantee at least one character from each required class is present in the output.
        /// </summary>
        IReadOnlyList<string> CharacterGroups { get; }
        int MaximumAttempts { get; }
        int MinimumLength { get; }
        int MaximumLength { get; }
        IPasswordSettings AddLowercase();
        IPasswordSettings AddUppercase();
        IPasswordSettings AddNumeric();
        IPasswordSettings AddSpecial();
        IPasswordSettings AddSpecial(string specialCharactersToAdd);
        string SpecialCharacters { get; set; }
    }
}
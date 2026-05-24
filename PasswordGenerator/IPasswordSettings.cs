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

        /// <summary>True when a custom pool (e.g. <see cref="UseCharacters" />) replaces the per-class sets.</summary>
        bool IsCustomPool { get; }

        /// <summary>When true, look-alike characters are removed from the pool before generating.</summary>
        bool ExcludeAmbiguous { get; }

        /// <summary>The minimum number of characters required from each class (empty when none are forced).</summary>
        IReadOnlyDictionary<CharacterClass, int> MinimumCounts { get; }

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

        /// <summary>Replaces the entire pool with an explicit set of characters (no forced composition).</summary>
        IPasswordSettings UseCharacters(string characters);

        /// <summary>Uses every printable ASCII character as the pool (no forced composition).</summary>
        IPasswordSettings UseAllAscii();

        /// <summary>Removes look-alike characters (see <see cref="CharacterFilter.AmbiguousCharacters" />) from the pool.</summary>
        IPasswordSettings ExcludeAmbiguousCharacters();

        /// <summary>Requires at least <paramref name="count" /> characters from the given class, enabling it if needed.</summary>
        IPasswordSettings RequireAtLeast(CharacterClass characterClass, int count);

        string SpecialCharacters { get; set; }
    }
}
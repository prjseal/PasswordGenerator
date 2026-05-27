using System.Collections.Generic;

namespace PasswordGenerator
{
    /// <summary>
    ///     Holds all of the settings for the password generator
    /// </summary>
    public interface IPasswordSettings
    {
        /// <summary>Whether lowercase letters are included in the pool.</summary>
        bool IncludeLowercase { get; }

        /// <summary>Whether uppercase letters are included in the pool.</summary>
        bool IncludeUppercase { get; }

        /// <summary>Whether digits are included in the pool.</summary>
        bool IncludeNumeric { get; }

        /// <summary>Whether special characters are included in the pool.</summary>
        bool IncludeSpecial { get; }

        /// <summary>The number of characters the generated password should contain.</summary>
        int PasswordLength { get; set; }

        /// <summary>The full set of characters the password is drawn from, after applying all settings.</summary>
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

        /// <summary>The maximum number of attempts allowed when generating a valid password.</summary>
        int MaximumAttempts { get; }

        /// <summary>The smallest allowed password length.</summary>
        int MinimumLength { get; }

        /// <summary>The largest allowed password length.</summary>
        int MaximumLength { get; }

        /// <summary>Enables lowercase letters in the pool.</summary>
        /// <returns>The same settings, for chaining.</returns>
        IPasswordSettings AddLowercase();

        /// <summary>Enables uppercase letters in the pool.</summary>
        /// <returns>The same settings, for chaining.</returns>
        IPasswordSettings AddUppercase();

        /// <summary>Enables digits in the pool.</summary>
        /// <returns>The same settings, for chaining.</returns>
        IPasswordSettings AddNumeric();

        /// <summary>Enables the default special characters in the pool.</summary>
        /// <returns>The same settings, for chaining.</returns>
        IPasswordSettings AddSpecial();

        /// <summary>Enables the given special characters in the pool.</summary>
        /// <param name="specialCharactersToAdd">The special characters to add to the pool.</param>
        /// <returns>The same settings, for chaining.</returns>
        IPasswordSettings AddSpecial(string specialCharactersToAdd);

        /// <summary>Replaces the entire pool with an explicit set of characters (no forced composition).</summary>
        /// <exception cref="System.ArgumentNullException"><paramref name="characters" /> is <see langword="null" />.</exception>
        IPasswordSettings UseCharacters(string characters);

        /// <summary>Uses every printable ASCII character as the pool (no forced composition).</summary>
        IPasswordSettings UseAllAscii();

        /// <summary>Removes look-alike characters (see <see cref="CharacterFilter.AmbiguousCharacters" />) from the pool.</summary>
        IPasswordSettings ExcludeAmbiguousCharacters();

        /// <summary>Requires at least <paramref name="count" /> characters from the given class, enabling it if needed.</summary>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        /// <exception cref="System.InvalidOperationException">A custom character pool is in use (per-class minimums cannot be combined with it).</exception>
        IPasswordSettings RequireAtLeast(CharacterClass characterClass, int count);

        /// <summary>The special characters used when special characters are included.</summary>
        string SpecialCharacters { get; set; }
    }
}
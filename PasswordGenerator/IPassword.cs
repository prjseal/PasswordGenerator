using System.Collections.Generic;

namespace PasswordGenerator
{
    /// <summary>
    ///     Fluent builder for configuring and generating passwords. Each configuration method returns the
    ///     same instance so calls can be chained.
    /// </summary>
    public interface IPassword
    {
        /// <summary>Includes lowercase letters in the pool.</summary>
        /// <returns>The same builder, for chaining.</returns>
        IPassword IncludeLowercase();

        /// <summary>Includes uppercase letters in the pool.</summary>
        /// <returns>The same builder, for chaining.</returns>
        IPassword IncludeUppercase();

        /// <summary>Includes digits in the pool.</summary>
        /// <returns>The same builder, for chaining.</returns>
        IPassword IncludeNumeric();

        /// <summary>Includes the default special characters in the pool.</summary>
        /// <returns>The same builder, for chaining.</returns>
        IPassword IncludeSpecial();

        /// <summary>Includes the given special characters in the pool.</summary>
        /// <param name="specialCharactersToInclude">The special characters to add to the pool.</param>
        /// <returns>The same builder, for chaining.</returns>
        IPassword IncludeSpecial(string specialCharactersToInclude);

        /// <summary>Replaces the pool with an explicit set of characters (no forced composition).</summary>
        /// <exception cref="System.ArgumentNullException"><paramref name="characters" /> is <see langword="null" />.</exception>
        IPassword WithCharacters(string characters);

        /// <summary>Uses every printable ASCII character as the pool (no forced composition).</summary>
        IPassword WithAllAscii();

        /// <summary>Removes look-alike characters from the pool (e.g. <c>I l 1 O 0 o</c>).</summary>
        IPassword ExcludeAmbiguous();

        /// <summary>Requires at least <paramref name="count" /> characters from the given class.</summary>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        /// <exception cref="System.InvalidOperationException">A custom character pool is in use (per-class minimums cannot be combined with it).</exception>
        IPassword RequireAtLeast(CharacterClass characterClass, int count);

        /// <summary>Sets the required password length.</summary>
        /// <param name="passwordLength">The number of characters the generated password should contain.</param>
        /// <returns>The same builder, for chaining.</returns>
        IPassword LengthRequired(int passwordLength);

        /// <summary>Generates a single password using the current settings.</summary>
        /// <returns>The generated password.</returns>
        string Next();

        /// <summary>Attempts to generate a single password without throwing on invalid settings.</summary>
        /// <param name="password">
        ///     When this method returns <see langword="true" />, the generated password; otherwise <see langword="null" />.
        /// </param>
        /// <returns><see langword="true" /> if a password was generated; otherwise <see langword="false" />.</returns>
        bool TryNext(out string? password);

        /// <summary>Generates a sequence of passwords using the current settings.</summary>
        /// <param name="numberOfPasswordsToGenerate">How many passwords to generate.</param>
        /// <returns>The generated passwords.</returns>
        IEnumerable<string> NextGroup(int numberOfPasswordsToGenerate);
    }
}
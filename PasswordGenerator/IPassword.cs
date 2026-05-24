using System.Collections.Generic;

namespace PasswordGenerator
{
    public interface IPassword
    {
        IPassword IncludeLowercase();
        IPassword IncludeUppercase();
        IPassword IncludeNumeric();
        IPassword IncludeSpecial();
        IPassword IncludeSpecial(string specialCharactersToInclude);

        /// <summary>Replaces the pool with an explicit set of characters (no forced composition).</summary>
        IPassword WithCharacters(string characters);

        /// <summary>Uses every printable ASCII character as the pool (no forced composition).</summary>
        IPassword WithAllAscii();

        /// <summary>Removes look-alike characters from the pool (e.g. <c>I l 1 O 0 o</c>).</summary>
        IPassword ExcludeAmbiguous();

        /// <summary>Requires at least <paramref name="count" /> characters from the given class.</summary>
        IPassword RequireAtLeast(CharacterClass characterClass, int count);

        IPassword LengthRequired(int passwordLength);
        string Next();
        bool TryNext(out string? password);
        IEnumerable<string> NextGroup(int numberOfPasswordsToGenerate);
    }
}
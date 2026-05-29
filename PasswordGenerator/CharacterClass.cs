namespace PasswordGenerator
{
    /// <summary>
    ///     The character classes a password can be composed from. Used by
    ///     <see cref="IPassword.RequireAtLeast" /> to guarantee a minimum number of characters per class.
    /// </summary>
    public enum CharacterClass
    {
        /// <summary>Lowercase letters (<c>a–z</c>).</summary>
        Lowercase,

        /// <summary>Uppercase letters (<c>A–Z</c>).</summary>
        Uppercase,

        /// <summary>Digits (<c>0–9</c>).</summary>
        Numeric,

        /// <summary>Special / symbol characters.</summary>
        Special
    }
}

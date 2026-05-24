namespace PasswordGenerator
{
    /// <summary>
    ///     The character classes a password can be composed from. Used by
    ///     <see cref="IPassword.RequireAtLeast" /> to guarantee a minimum number of characters per class.
    /// </summary>
    public enum CharacterClass
    {
        Lowercase,
        Uppercase,
        Numeric,
        Special
    }
}

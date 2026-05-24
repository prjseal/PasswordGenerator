namespace PasswordGenerator
{
    /// <summary>
    ///     Configuration for a password generator, used when registering via dependency injection or
    ///     binding from configuration (e.g. appSettings.json).
    /// </summary>
    public class PasswordOptions
    {
        public bool IncludeLowercase { get; set; } = true;
        public bool IncludeUppercase { get; set; } = true;
        public bool IncludeNumeric { get; set; } = true;
        public bool IncludeSpecial { get; set; } = true;

        /// <summary>Custom special characters. When null/empty the library default special set is used.</summary>
        public string? SpecialCharacters { get; set; }

        public int Length { get; set; } = 16;
    }
}

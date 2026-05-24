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

        /// <summary>Removes look-alike characters from the pool when true.</summary>
        public bool ExcludeAmbiguous { get; set; }

        /// <summary>The number of passwords produced by the parameterless <c>Generate()</c> overload.</summary>
        public int DefaultBatchCount { get; set; } = 1;
    }
}

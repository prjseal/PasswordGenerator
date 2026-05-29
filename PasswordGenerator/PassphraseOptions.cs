namespace PasswordGenerator
{
    /// <summary>
    ///     Configuration for passphrase generation, used when registering via dependency injection or
    ///     binding from configuration (e.g. the "Passphrase" section of appSettings.json). When this is
    ///     set on <see cref="PasswordOptions" />, the registered generator produces passphrases.
    /// </summary>
    public class PassphraseOptions
    {
        /// <summary>The number of words in each passphrase. Defaults to <c>4</c>.</summary>
        public int WordCount { get; set; } = 4;

        /// <summary>
        ///     The character placed between words. Defaults to <c>'-'</c>. Set to <see langword="null" />
        ///     (or an empty string in configuration) for no separator.
        /// </summary>
        public char? Separator { get; set; } = '-';

        /// <summary>Whether the first letter of each word is capitalized.</summary>
        public bool Capitalize { get; set; }

        /// <summary>Whether a random two-digit number is appended. Defaults to <see langword="true" />.</summary>
        public bool IncludeNumber { get; set; } = true;

        /// <summary>Whether a random symbol is attached to one randomly chosen word.</summary>
        public bool IncludeSymbol { get; set; }

        /// <summary>An optional entropy floor in bits; zero (the default) means no floor.</summary>
        public double MinimumEntropyBits { get; set; }
    }
}

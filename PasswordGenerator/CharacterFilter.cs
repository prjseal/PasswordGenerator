using System.Text;

namespace PasswordGenerator
{
    /// <summary>
    ///     Shared character-pool helpers: well-known pools and ambiguous-character removal.
    /// </summary>
    public static class CharacterFilter
    {
        /// <summary>Look-alike characters removed by <see cref="IPassword.ExcludeAmbiguous" />.</summary>
        public const string AmbiguousCharacters = "Il1O0o";

        /// <summary>URL-safe characters (RFC 4648 base64url alphabet), used by API-key style secrets.</summary>
        public const string UrlSafeCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        private static string? _allPrintableAscii;

        /// <summary>All printable ASCII characters (code points 33-126; excludes space).</summary>
        public static string AllPrintableAscii
        {
            get
            {
                if (_allPrintableAscii != null) return _allPrintableAscii;

                var sb = new StringBuilder(126 - 33 + 1);
                for (var c = 33; c <= 126; c++)
                    sb.Append((char)c);
                _allPrintableAscii = sb.ToString();
                return _allPrintableAscii;
            }
        }

        /// <summary>Returns <paramref name="input" /> with ambiguous characters removed when <paramref name="exclude" /> is true.</summary>
        public static string RemoveAmbiguous(string? input, bool exclude)
        {
            if (!exclude || string.IsNullOrEmpty(input))
                return input ?? string.Empty;

            var sb = new StringBuilder(input!.Length);
            foreach (var ch in input)
                if (AmbiguousCharacters.IndexOf(ch) < 0)
                    sb.Append(ch);

            return sb.ToString();
        }
    }
}

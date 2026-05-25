using System;
using System.Security.Cryptography;

namespace PasswordGenerator
{
    /// <summary>
    ///     <see cref="IRandomSource" /> backed by a cryptographic RNG. Uses
    ///     <see cref="RandomNumberGenerator.GetInt32(int)" />, which samples uniformly across the whole
    ///     range with no modulo bias.
    /// </summary>
    public sealed class CryptoRandomSource : IRandomSource
    {
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be positive.");

            return RandomNumberGenerator.GetInt32(maxExclusive);
        }
    }
}

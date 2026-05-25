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
        /// <summary>
        ///     Returns a uniformly distributed, non-negative random integer that is less than
        ///     <paramref name="maxExclusive" />.
        /// </summary>
        /// <param name="maxExclusive">The exclusive upper bound; must be positive.</param>
        /// <returns>A random integer in the range <c>[0, maxExclusive)</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="maxExclusive" /> is zero or negative.
        /// </exception>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be positive.");

            return RandomNumberGenerator.GetInt32(maxExclusive);
        }
    }
}

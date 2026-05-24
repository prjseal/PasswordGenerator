using System;
using System.Security.Cryptography;

namespace PasswordGenerator
{
    /// <summary>
    ///     <see cref="IRandomSource" /> backed by a cryptographic RNG.
    ///     On modern targets it uses <see cref="RandomNumberGenerator.GetInt32(int)" />; on
    ///     <c>netstandard2.0</c> it uses rejection sampling so the result is uniform across the whole
    ///     range with no modulo bias and no off-by-one.
    /// </summary>
    public sealed class CryptoRandomSource : IRandomSource, IDisposable
    {
#if !NET8_0_OR_GREATER
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
#endif

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be positive.");

#if NET8_0_OR_GREATER
            return RandomNumberGenerator.GetInt32(maxExclusive);
#else
            if (maxExclusive == 1) return 0;

            var range = (uint)maxExclusive;

            // Largest multiple of range that is <= 2^32. Values at or above this are rejected so the
            // accepted region is a whole number of buckets, giving an unbiased result mod range.
            const ulong fullSpace = 1UL << 32;
            var limit = fullSpace - fullSpace % range;

            uint value;
            do
            {
                value = NextUInt32();
            } while (value >= limit);

            return (int)(value % range);
#endif
        }

#if !NET8_0_OR_GREATER
        private uint NextUInt32()
        {
            var buffer = new byte[4];
            _rng.GetBytes(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }
#endif

        public void Dispose()
        {
#if !NET8_0_OR_GREATER
            _rng.Dispose();
#endif
        }
    }
}

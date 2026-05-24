using System;
using System.Security.Cryptography;

namespace PasswordGenerator
{
    /// <summary>
    ///     <see cref="IRandomSource" /> backed by a cryptographic RNG. Uses rejection sampling so the
    ///     result is uniform across the whole range with no modulo bias and no off-by-one.
    /// </summary>
    public sealed class CryptoRandomSource : IRandomSource, IDisposable
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be positive.");

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
        }

        private uint NextUInt32()
        {
            var buffer = new byte[4];
            _rng.GetBytes(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public void Dispose()
        {
            _rng.Dispose();
        }
    }
}

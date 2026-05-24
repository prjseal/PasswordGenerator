using System;
using System.Security.Cryptography;

namespace PasswordGenerator
{
    /// <summary>
    ///     An <see cref="IRandomSource" /> backed by a cryptographically secure
    ///     <see cref="RandomNumberGenerator" />. Uses rejection sampling so selection is
    ///     uniform across the whole range (no modulo bias, no off-by-one).
    /// </summary>
    public sealed class CryptoRandomSource : IRandomSource
    {
        private readonly RandomNumberGenerator _rng;

        public CryptoRandomSource() : this(RandomNumberGenerator.Create())
        {
        }

        public CryptoRandomSource(RandomNumberGenerator rng)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be positive.");

            if (maxExclusive == 1)
                return 0;

            var range = (uint)maxExclusive;

            // Largest multiple of range that fits in the 2^32 sample space; sampled values at or
            // above this are rejected so the accepted values divide evenly (uniform, no bias).
            var limit = 0x1_0000_0000UL / range * range;

            var bytes = new byte[sizeof(uint)];
            uint value;
            do
            {
                _rng.GetBytes(bytes);
                value = BitConverter.ToUInt32(bytes, 0);
            } while (value >= limit);

            return (int)(value % range);
        }
    }
}

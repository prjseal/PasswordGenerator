using System;

namespace PasswordGenerator
{
    /// <summary>
    ///     Default <see cref="IEntropyEstimator" />: entropy = length * log2(effective pool size).
    /// </summary>
    public class PoolEntropyEstimator : IEntropyEstimator
    {
        /// <summary>
        ///     Estimates the entropy, in bits, of passwords produced with the given
        ///     <paramref name="settings" /> as <c>length × log2(effective pool size)</c>.
        /// </summary>
        /// <param name="settings">The settings describing the character pool and length.</param>
        /// <returns>
        ///     The estimated entropy in bits, or <c>0</c> when the pool is empty or the length is not positive.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="settings" /> is <see langword="null" />.</exception>
        public double EstimateBits(IPasswordSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var pool = CharacterFilter.RemoveAmbiguous(settings.CharacterSet, settings.ExcludeAmbiguous);
            if (string.IsNullOrEmpty(pool) || settings.PasswordLength <= 0)
                return 0d;

            return settings.PasswordLength * Math.Log(pool.Length, 2);
        }
    }
}

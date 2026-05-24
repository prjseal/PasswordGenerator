using System;

namespace PasswordGenerator
{
    /// <summary>
    ///     Default <see cref="IEntropyEstimator" />: entropy = length * log2(effective pool size).
    /// </summary>
    public class PoolEntropyEstimator : IEntropyEstimator
    {
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

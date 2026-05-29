namespace PasswordGenerator
{
    /// <summary>
    ///     Estimates the strength, in bits, of passwords produced from a given set of settings.
    /// </summary>
    public interface IEntropyEstimator
    {
        /// <summary>
        ///     Estimates entropy in bits as <c>length * log2(poolSize)</c>, using the effective character
        ///     pool (after any ambiguous-character exclusion). This is an upper-bound estimate that ignores
        ///     forced-composition minimums.
        /// </summary>
        double EstimateBits(IPasswordSettings settings);
    }
}

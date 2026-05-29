namespace PasswordGenerator
{
    /// <summary>
    ///     Source of random integers used to build passwords. Abstracted so the crypto RNG can be
    ///     swapped for a deterministic source in tests and wired through DI.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        ///     Returns a uniformly distributed integer in the range [0, maxExclusive).
        /// </summary>
        int NextInt(int maxExclusive);
    }
}

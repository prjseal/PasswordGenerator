namespace PasswordGenerator
{
    /// <summary>
    ///     A source of uniformly distributed random integers used to select characters.
    ///     Abstracted so generation can be driven by a cryptographic source in production
    ///     and a deterministic stub in tests.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        ///     Returns a uniformly distributed integer in the range [0, <paramref name="maxExclusive" />).
        /// </summary>
        int NextInt(int maxExclusive);
    }
}

namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using PRNGLib;

    /// <summary>
    ///     RandomType - we use separate RNGs for gaming (outcome) and non-gaming purposes.
    /// </summary>
    public enum RandomType
    {
        /// <summary>
        ///     Gaming - use for gaming outcome generation.
        /// </summary>
        Gaming,

        /// <summary>
        ///     NonGaming - use for all other purposes.
        /// </summary>
        NonGaming
    }

    /// <summary>
    ///     Provides a mechanism to get an RNG of the requested type.
    /// </summary>
    [CLSCompliant(false)]
    public interface IRandomFactory
    {
        /// <summary>
        ///     GetRNG() - get an RNG of the requested type.
        /// </summary>
        /// <param name="type">Gaming or NonGaming.</param>
        /// <returns>A random number generator implementation.</returns>
        IPRNG Create(RandomType type);
    }
}
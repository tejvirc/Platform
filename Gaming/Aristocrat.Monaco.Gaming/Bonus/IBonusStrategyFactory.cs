namespace Aristocrat.Monaco.Gaming.Bonus
{
    using Contracts.Bonus;

    /// <summary>
    ///     Factory for bonus strategies
    /// </summary>
    public interface IBonusStrategyFactory
    {
        /// <summary>
        ///     Creates the bonus strategy for the specified mode
        /// </summary>
        /// <param name="mode">The bonus mode</param>
        /// <returns>The bonus strategy</returns>
        IBonusStrategy Create(BonusMode mode);
    }
}
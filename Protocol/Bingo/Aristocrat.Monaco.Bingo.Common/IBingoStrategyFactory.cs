namespace Aristocrat.Monaco.Bingo.Common
{
    /// <summary>
    ///     A generic strategy factory that can be used
    /// </summary>
    /// <typeparam name="TStrategy">The strategy that will be created</typeparam>
    /// <typeparam name="TStrategyType">The strategy type used as the key for creating a new strategy</typeparam>
    public interface IBingoStrategyFactory<out TStrategy, in TStrategyType>
        where TStrategy : class
    {
        /// <summary>
        ///     Creates the request strategy based on the strategy type
        /// </summary>
        /// <param name="strategyType">The strategy type to create</param>
        /// <returns>The strategy created or null if none is found</returns>
        TStrategy Create(TStrategyType strategyType);
    }
}
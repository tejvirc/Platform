namespace Aristocrat.Monaco.Gaming.UI.CompositionRoot
{
    using Contracts;

    public interface IOverlayMessageStrategyFactory
    {
        /// <summary>
        ///     Creates the strategyOptions for the specified mode
        /// </summary>
        /// <param name="strategyOptions">The strategy option</param>
        /// <returns>The overlay message strategyOptions</returns>
        IOverlayMessageStrategy Create(OverlayMessageStrategyOptions strategyOptions);
    }
}

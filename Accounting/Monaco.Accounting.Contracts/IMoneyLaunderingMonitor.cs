namespace Aristocrat.Monaco.Accounting.Contracts
{
    /// <summary>
    /// Interface for MoneyLaunderingMonitor
    /// </summary>
    public interface IMoneyLaunderingMonitor
    {
        /// <summary>
        /// Determines if the threshold has been reached
        /// </summary>
        bool IsThresholdReached();

        /// <summary>
        /// Used by gaming layer to notify that player has started a game
        /// </summary>
        void NotifyGameStarted();
    }
}

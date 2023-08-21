namespace Aristocrat.Monaco.Asp.Progressive
{
    using System;
    using System.Collections.Generic;
    using Gaming.Contracts.Progressives;

    /// <summary>
    /// Holds protocol level link progressive state and functionality to process jackpot wins.
    /// </summary>
    public interface IProgressiveManager : IProtocolProgressiveEventHandler, IDisposable
    {
        /// <summary>
        /// Link Progressive state for each supported level
        /// </summary>
        IReadOnlyDictionary<int, ILinkProgressiveLevel> Levels { get; }

        /// <summary>
        /// Event fired when notifications are available for progressive controller
        /// </summary>
        event EventHandler<OnNotificationEventArgs> OnNotificationEvent;

        /// <summary>
        /// Sets the jackpot amount sent from progressive controller (C5T9P5 / C5T0AP5 / C5T0BP5 / C5T0CP5)
        /// </summary>
        /// <param name="levelId">The progressive level id</param>
        /// <param name="amount">The amount in cents</param>
        void UpdateLinkJackpotHitAmountWon(int levelId, long amount);

        /// <summary>
        /// Sets the jackpot pool totals sent from progressive controller (C5T9P6 + C5T9P0AD1 / C5T0AP6 + C5T0AP0AD1 / C5T0BP6 + C5T0BP0AD1 / C5T0CP6 + C5T0CP0AD1)
        /// </summary>
        /// <param name="levelId">The progressive level id</param>
        /// <param name="amount">The amount in cents</param>
        void UpdateProgressiveJackpotAmountUpdate(int levelId, long amount);

        /// <summary>
        /// Updates jackpot amounts
        /// </summary>
        /// <param name="amounts">The amounts to update</param>
        void UpdateProgressiveJackpotAmountUpdate(Dictionary<int, long> amounts);

        /// <summary>
        /// Gets amounts per level
        /// </summary>
        /// <returns>The level amounts</returns>
        Dictionary<int, LevelAmountUpdateState> GetJackpotAmountsPerLevel();
    }
}
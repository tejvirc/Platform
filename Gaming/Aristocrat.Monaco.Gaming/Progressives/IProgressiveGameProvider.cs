namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System.Collections.Generic;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;

    /// <summary>
    ///     Provides a progressive interface for game related behavior such as loading progressive data,
    ///     activating current progressive levels, incrementing level values, triggering wins,
    ///     and claiming the jackpot
    /// </summary>
    public interface IProgressiveGameProvider
    {
        /// <summary>
        ///     Activates all of the progressive levels associated with the provided game id and denominations
        /// </summary>
        /// <param name="packName">The name of the progressive pack</param>
        /// <param name="gameId">The unique id of the game being initialized</param>
        /// <param name="denomination">The denomination of the game being initialized</param>
        /// <param name="betOption">The bet option of the game being initialized</param>
        /// <returns>An enumeration of all progressive levels that were activated</returns>
        IEnumerable<IViewableProgressiveLevel> ActivateProgressiveLevels(
            string packName,
            int gameId,
            long denomination,
            string betOption);

        /// <summary>
        ///     Deactivates all of the progressive levels associated with the provided game id and denominations
        /// </summary>
        /// <param name="packName">The name of the progressive pack</param>
        /// <returns>An enumeration of all progressive levels that were deactivated</returns>
        IEnumerable<IViewableProgressiveLevel> DeactivateProgressiveLevels(string packName);

        /// <summary>
        ///     Gets a snapshot of the active progressive pack
        /// </summary>
        /// <param name="packName">The name of the progressive pack</param>
        /// <returns>An enumeration of summarized progressive values</returns>
        IEnumerable<Jackpot> GetJackpotSnapshot(string packName);

        /// <summary>
        ///     Gets the list of active progressive levels
        /// </summary>
        /// <returns>The active progressive levels</returns>
        IEnumerable<IViewableProgressiveLevel> GetActiveProgressiveLevels();

        /// <summary>
        ///     Gets the list of active linked progressive levels
        /// </summary>
        /// <returns>The active linked progressive levels</returns>
        IEnumerable<IViewableLinkedProgressiveLevel> GetActiveLinkedProgressiveLevels();

        /// <summary>
        ///     Bulk increments a progressive pack
        /// </summary>
        /// <param name="packName">The name of the progressive pack</param>
        /// <param name="wager">The wager for the progressive update</param>
        /// <param name="ante">The ante for the progressive update</param>
        void IncrementProgressiveLevel(string packName, long wager, long ante);

        /// <summary>
        ///     Sets the progressive levels wager amounts
        /// </summary>
        /// <param name="levelWagers">The wager amounts to use for the incrementing</param>
        void SetProgressiveWagerAmounts(IReadOnlyList<long> levelWagers);

        /// <summary>
        ///     Increments the progressive levels using provided progressive level data
        /// </summary>
        /// <param name="packName">The name of the progressive pack</param>
        /// <param name="levelUpdates">The level update data</param>
        void IncrementProgressiveLevelPack(string packName, IEnumerable<ProgressiveLevelUpdate> levelUpdates);

        /// <summary>
        ///     Updates game progressive levels that are linked
        /// </summary>
        /// <param name="linkedLevelUpdates">Linked progressive level updates</param>
        void UpdateLinkedProgressiveLevels(IEnumerable<IViewableLinkedProgressiveLevel> linkedLevelUpdates);

        /// <summary>
        ///     Gets a current mapping of progressive levels to amounts for a given progressive pack name
        /// </summary>
        /// <param name="packName">The name of the progressive pack</param>
        /// <param name="isRecovering">true if this is being called during game recovery</param>
        /// <returns>A mapping of progressive level ids to their current values</returns>
        IDictionary<int, long> GetProgressiveLevel(string packName, bool isRecovering);

        /// <summary>
        /// </summary>
        /// <param name="packName">The name of the progressive pack</param>
        /// <param name="levelIds">The list of levels being triggered</param>
        /// <param name="transactionIds">A list of transactions previously returned to the runtime</param>
        /// <param name="recovering">Whether or not we are recovering</param>
        /// <returns>A collection of trigger results</returns>
        IEnumerable<ProgressiveTriggerResult> TriggerProgressiveLevel(
            string packName,
            IList<int> levelIds,
            IList<long> transactionIds,
            bool recovering);

        /// <summary>
        ///     Claims the results of the provided transaction Ids.  This should be invoked when the credits hit the credit meter
        /// </summary>
        /// <param name="packName">The name of the progressive pack</param>
        /// <param name="transactionIds">A list of transactions previously returned to the runtime</param>
        /// <returns>A collection of results</returns>
        IEnumerable<ClaimResult> ClaimProgressiveLevel(string packName, IList<long> transactionIds);

        /// <summary>
        ///     Commit pending progressive payouts. If a payout cannot be completed, it will be returned.
        /// </summary>
        /// <param name="pendingProgressives">The pending progressive payouts.</param>
        /// <returns>Any pending progressive payout that failed to be committed.</returns>
        IEnumerable<PendingProgressivePayout> CommitProgressiveWin(
            IReadOnlyCollection<PendingProgressivePayout> pendingProgressives);

        /// <summary>
        ///     Sets the final win for a triggered progressive
        /// </summary>
        /// <param name="transactionId">The transaction Id of the triggered progressive</param>
        /// <param name="winAmount">The win amount</param>
        /// <param name="winText">The win text</param>
        /// <param name="payMethod">The pay method for the progressive</param>
        void SetProgressiveWin(long transactionId, long winAmount, string winText, PayMethod payMethod);

        /// <summary>
        ///     Increments the progressive win meters.
        /// </summary>
        /// <param name="pendingProgressivePayouts">The pending progressive payouts.</param>
        void IncrementProgressiveWinMeters(IReadOnlyCollection<PendingProgressivePayout> pendingProgressivePayouts);
    }
}
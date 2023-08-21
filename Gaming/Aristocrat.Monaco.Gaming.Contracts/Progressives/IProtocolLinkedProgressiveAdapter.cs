namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Linked;

    /// <summary>
    ///     Provides an adapter interface for the protocol layer to communicate with progressives
    ///     in order to prevent API calls from protocols not configured to handle progressives.
    ///     Filters out all API calls made to ILinkedProgressiveProvider and IProgressiveConfigurationProvider.
    /// 
    ///     Most API calls require the protocolName to be included which be used for filtering.
    ///     Any protocol can view progressive data which is why view calls don't have a progressiveName parameter.
    /// </summary>
    public interface IProtocolLinkedProgressiveAdapter
    {
        /// <summary>
        ///     Notify the system that host for a given protocol name is down
        /// </summary>
        /// <param name="protocolName">The protocol that is down</param>
        void ReportLinkDown(string protocolName);

        /// <summary>
        ///     Notify the system that host for a given protocol name is back up
        /// </summary>
        /// <param name="protocolName">The protocol that is up</param>
        void ReportLinkUp(string protocolName);

        /// <summary>
        ///     Update linked progressive levels
        /// </summary>
        /// <param name="levelUpdates">The linked levels to update</param>
        /// <param name="protocolName">The protocol that is calling API</param>
        void UpdateLinkedProgressiveLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelUpdates, string protocolName);

        /// <summary>
        ///     Update linked progressive levels asynchronously
        /// </summary>
        /// <param name="levelUpdates">The linked levels to update</param>
        /// <param name="protocolName">The protocol that is calling API</param>
        // ReSharper disable once UnusedMethodReturnValue.Global
        Task UpdateLinkedProgressiveLevelsAsync(IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelUpdates, string protocolName);

        /// <summary>
        ///     View currently available linked progressive levels
        /// </summary>
        /// <returns>Linked progressive levels</returns>
        // ReSharper disable once UnusedMethodReturnValue.Global
        IReadOnlyCollection<IViewableLinkedProgressiveLevel> ViewLinkedProgressiveLevels();

        /// <summary>
        ///     Attempts to retrieve a linked progressive level by its name
        /// </summary>
        /// <param name="levelName">The unique levels name</param>
        /// <param name="level">The matching linked level if valid</param>
        /// <returns>True if the level is found. False if not found</returns>
        bool ViewLinkedProgressiveLevel(string levelName, out IViewableLinkedProgressiveLevel level);

        /// <summary>
        ///     Claim a linked progressive level. This call must be done on any level
        ///     that can be awarded.
        /// </summary>
        /// <param name="levelName">The name of the level to claim</param>
        /// <param name="protocolName">The protocol that is calling API</param>
        /// <returns>Returns false if the claim attempt fails</returns>
        void ClaimLinkedProgressiveLevel(string levelName, string protocolName);

        /// <summary>
        ///     Award a claimed linked progressive level.
        /// </summary>
        void AwardLinkedProgressiveLevel(string levelName, string protocolName);

        /// <summary>
        ///     Award a claimed linked progressive level.
        /// </summary>
        void AwardLinkedProgressiveLevel(string levelName, long winAmount, string protocolName);

        /// <summary>
        ///     View all progressive levels
        /// </summary>
        /// <returns>All progressive levels</returns>
        IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels();

        /// <summary>
        ///     View filtered progressive levels
        /// </summary>
        /// <param name="gameId">The id of the game associated with the progressive levels</param>
        /// <param name="denom">The denom of the game associated with the progressive levels</param>
        /// <param name="progressivePackName">The pack name associated with the progressive levels</param>
        /// <returns>All progressive levels</returns>
        IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels(int gameId, long denom, string progressivePackName);

        /// <summary>
        ///     Assigns an assignable progressive level to a progressive level defined by a game
        /// </summary>
        /// <param name="levelAssignments">The level assignments to be processed</param>
        /// <param name="protocolName">The protocol that is calling API</param>
        /// <returns>The resulting assignment operation</returns>
        IReadOnlyCollection<IViewableProgressiveLevel> AssignLevelsToGame(IReadOnlyCollection<ProgressiveLevelAssignment> levelAssignments, string protocolName);

        /// <summary>
        ///     View progressive levels that have been configured
        /// </summary>
        /// <returns>All progressive levels that have been configured</returns>
        IEnumerable<IViewableProgressiveLevel> ViewConfiguredProgressiveLevels();

        /// <summary>
        ///     View progressive levels that have been configured by game id and denom
        /// </summary>
        /// <param name="gameId">The id of the game associated with the progressive levels</param>
        /// <param name="denom">The denom of the game associated with the progressive levels</param>
        /// <returns>All progressive levels that have been configured</returns>
        IEnumerable<IViewableProgressiveLevel> ViewConfiguredProgressiveLevels(int gameId, long denom);

        /// <summary>
        ///     Gets the list of active progressive levels
        /// </summary>
        /// <returns>The active progressive levels</returns>
        IEnumerable<IViewableProgressiveLevel> GetActiveProgressiveLevels();

        /// <summary>
        ///    Transitions linked level claim status from claimed to awarded in one scoped transaction
        ///    This call must be done on any level that can be awarded
        /// </summary>
        /// <param name="levelName">The name of the level to claim</param>
        /// <param name="protocolName">The protocol that is calling API</param>
        /// <param name="winAmount">The winAmount that should be used to award</param>
        void ClaimAndAwardLinkedProgressiveLevel(string protocolName, string levelName, long winAmount = -1);
    }
}

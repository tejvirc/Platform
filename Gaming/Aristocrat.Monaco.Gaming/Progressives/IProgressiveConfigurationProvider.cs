namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System.Collections.Generic;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;

    /// <summary>
    ///     Provides an interface to configure progressive levels
    ///     In order to use this interface in the protocol layer, use the IProtocolLinkedProgressiveAdapter
    /// </summary>
    public interface IProgressiveConfigurationProvider
    {
        /// <summary>
        ///     Assigns an assignable progressive level to a progressive level defined by a game
        /// </summary>
        /// <param name="levelAssignments">The level assignments to be processed</param>
        /// <returns>The resulting assignment operation</returns>
        IReadOnlyCollection<IViewableProgressiveLevel> AssignLevelsToGame(IReadOnlyCollection<ProgressiveLevelAssignment> levelAssignments);

        /// <summary>
        ///     The progressives levels to lock preventing configuration
        /// </summary>
        /// <param name="progressiveLevels">The levels to lock</param>
        void LockProgressiveLevels(IReadOnlyCollection<IViewableProgressiveLevel> progressiveLevels);

        /// <summary>
        ///     Validates the linked progressive level and returns any errors for that level
        /// </summary>
        /// <param name="linkedProgressiveLevel">The linked progressive level to validate</param>
        /// <param name="progressiveLevel">The associated progressive level to evaluate it against</param>
        /// <returns>The errors for the given progressive level</returns>
        ProgressiveErrors ValidateLinkedProgressive(
            IViewableLinkedProgressiveLevel linkedProgressiveLevel,
            IViewableProgressiveLevel progressiveLevel);

        /// <summary>
        ///     Validates the linked progressive level update
        /// </summary>
        /// <param name="levels">The levels to validate</param>
        void ValidateLinkedProgressivesUpdates(IEnumerable<IViewableLinkedProgressiveLevel> levels);

        /// <summary>
        ///     View all progressive levels
        /// </summary>
        /// <returns>All progressive levels</returns>
        IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels();

        /// <summary>
        ///     View all progressive levels by game id, denom, and pack name
        /// </summary>
        /// <param name="gameId">The id of the game associated with the progressive levels</param>
        /// <param name="denom">The denom of the game associated with the progressive levels</param>
        /// <param name="progressivePackName">The name of the pack associated with the progressive levels</param>
        /// <returns></returns>
        IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels(
            int gameId,
            long denom,
            string progressivePackName);

        /// <summary>
        ///     View all progressive levels by game id, denom
        /// </summary>
        /// <param name="gameId">The id of the game associated with the progressive levels</param>
        /// <param name="denom">The denom of the game associated with the progressive levels</param>
        /// <returns></returns>
        IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels(int gameId, long denom);

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
        ///     View shared progressive levels
        /// </summary>
        /// <returns>A readonly collection of shared sap levels</returns>
        IReadOnlyCollection<IViewableSharedSapLevel> ViewSharedSapLevels();

        /// <summary>
        ///     View linked progressive levels
        /// </summary>
        /// <returns>A readonly collection of linked progressive levels</returns>
        IReadOnlyCollection<IViewableLinkedProgressiveLevel> ViewLinkedProgressiveLevels();
    }
}
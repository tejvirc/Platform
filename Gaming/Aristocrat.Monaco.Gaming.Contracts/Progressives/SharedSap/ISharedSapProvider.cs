namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.SharedSap
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides an interface to add, remove, view, and update shared sap levels
    /// </summary>
    public interface ISharedSapProvider
    {
        /// <summary>
        ///     Add one or more shared sap levels
        /// </summary>
        /// <param name="sharedSapLevels">The shared sap levels to be added</param>
        /// <returns>Levels that were successfully added</returns>
        IEnumerable<IViewableSharedSapLevel> AddSharedSapLevel(IEnumerable<IViewableSharedSapLevel> sharedSapLevels);

        /// <summary>
        ///     Remove one or more shared sap levels
        /// </summary>
        /// <param name="sharedSapLevels">The shared sap levels to be removed</param>
        /// <returns>Levels that were successfully removed</returns>
        IEnumerable<IViewableSharedSapLevel> RemoveSharedSapLevel(IEnumerable<IViewableSharedSapLevel> sharedSapLevels);

        /// <summary>
        ///     Updates one or more shared sap levels
        /// </summary>
        /// <param name="levelUpdates">The levels that will be updated based on name</param>
        /// <returns>The levels that were successfully updated</returns>
        IEnumerable<IViewableSharedSapLevel> UpdateSharedSapLevel(IEnumerable<IViewableSharedSapLevel> levelUpdates);

        /// <summary>
        ///     Increments a progressive level using a wager
        /// </summary>
        /// <param name="level">The level to increment</param>
        /// <param name="wager">The wager amount</param>
        /// <param name="ante">The ante</param>
        void Increment(ProgressiveLevel level, long wager, long ante);

        /// <summary>
        ///     Processes a hit on a progressive level
        /// </summary>
        /// <param name="level">The level with the hit</param>
        /// <param name="transaction">The associated transaction information</param>
        void ProcessHit(ProgressiveLevel level, IViewableJackpotTransaction transaction);

        /// <summary>
        ///     Resets a progressive level.
        /// </summary>
        /// <param name="level">The level to reset</param>
        void Reset(ProgressiveLevel level);

        /// <summary>
        ///     View all shared sap levels
        /// </summary>
        /// <returns>All shared levels active in memory.</returns>
        IEnumerable<IViewableSharedSapLevel> ViewSharedSapLevels();

        /// <summary>
        ///     Attempts to get the shared sap progressive level for the provided level name
        /// </summary>
        /// <param name="assignmentKey">The assignment key for this level</param>
        /// <param name="level">The matching linked level if valid</param>
        /// <returns>True if the level is found. False if not found</returns>
        bool ViewSharedSapLevel(string assignmentKey, out IViewableSharedSapLevel level);

        /// <summary>
        ///     Saves all shared sap levels to persistence
        /// </summary>
        /// <returns>The levels that were successfully saved</returns>
        IEnumerable<IViewableSharedSapLevel> Save();

        /// <summary>
        ///     Associate levels to shared level
        /// </summary>
        /// <param name="levels">The levels to associate</param>
        void AssociateLevels(IList<ProgressiveLevel> levels);
    }
}
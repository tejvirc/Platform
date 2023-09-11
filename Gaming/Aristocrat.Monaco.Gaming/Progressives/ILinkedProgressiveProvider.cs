namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;

    /// <summary>
    ///     Provides and interface to add, remove, and update linked progressive levels
    ///     In order to use this interface in the protocol layer, use the IProtocolLinkedProgressiveAdapter
    /// </summary>
    public interface ILinkedProgressiveProvider
    {
        /// <summary>
        ///     Adds a set of linked progressive levels based on group id and level id. Group Id
        ///     and level id must be unique.
        /// </summary>
        /// <param name="linkedProgressiveLevels">The linked levels to be added</param>
        void AddLinkedProgressiveLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> linkedProgressiveLevels);

        /// <summary>
        ///     Attempts to remove a set of linked progressive levels based on group id and level id.
        /// </summary>
        /// <param name="linkedProgressiveLevels">The linked levels to be moved</param>
        void RemoveLinkedProgressiveLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> linkedProgressiveLevels);

        /// <summary>
        ///     Update linked progressive levels
        /// </summary>
        /// <param name="levelUpdates">The linked levels to update</param>
        void UpdateLinkedProgressiveLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelUpdates);

        /// <summary>
        ///     Update linked progressive levels asynchronously
        /// </summary>
        /// <param name="levelUpdates">The linked levels to update</param>
        Task UpdateLinkedProgressiveLevelsAsync(IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelUpdates);

        /// <summary>
        ///     Notify the system that the host for a given group id is down
        /// </summary>
        /// <param name="progressiveGroupId">The group id with the link down</param>
        void ReportLinkDown(int progressiveGroupId);

        /// <summary>
        ///     Notify the system that host for a given protocol name is down
        /// </summary>
        /// <param name="protocolName">The protocol that is down</param>
        void ReportLinkDown(string protocolName);

        /// <summary>
        ///     Notify the system that the host for a given group is up
        /// </summary>
        /// <param name="progressiveGroupId">The group id with the link up</param>
        void ReportLinkUp(int progressiveGroupId);

        /// <summary>
        ///     Notify the system that host for a given protocol name is back up
        /// </summary>
        /// <param name="protocolName">The protocol that is up</param>
        void ReportLinkUp(string protocolName);

        /// <summary>
        ///     Claim a linked progressive level. This call must be done on any level
        ///     that can be awarded.
        /// </summary>
        /// <param name="levelName">The name of the level to claim</param>
        /// <returns>Returns false if the claim attempt fails</returns>
        void ClaimLinkedProgressiveLevel(string levelName);

        /// <summary>
        ///     Award a claimed linked progressive level.
        /// </summary>
        void AwardLinkedProgressiveLevel(string levelName, long winAmount = -1);

        /// <summary>
        ///     Awards claimed level asynchronously 
        /// </summary>
        Task AwardLinkedProgressiveLevelAsync(string levelName);

        /// <summary>
        ///     Resolves the awarded level. 
        /// </summary>
        /// <param name="levelName">The name of the level to resolve</param>
        /// <param name="transaction">The associated transaction</param>
        void Reset(string levelName, IViewableJackpotTransaction transaction);

        /// <summary>
        ///     View currently available linked progressive levels
        /// </summary>
        /// <returns>Linked progressive levels</returns>
        IReadOnlyCollection<IViewableLinkedProgressiveLevel> ViewLinkedProgressiveLevels();

        /// <summary>
        ///     Attempts to retrieve a linked progressive level by its name
        /// </summary>
        /// <param name="levelName">The unique levels name</param>
        /// <param name="level">The matching linked level if valid</param>
        /// <returns>True if the level is found. False if not found</returns>
        bool ViewLinkedProgressiveLevel(string levelName, out IViewableLinkedProgressiveLevel level);

        /// <summary>
        ///     Attempts to retrieve a group of linked progressive levels by their names
        /// </summary>
        /// <param name="levelNames">The unique levels name</param>
        /// <param name="levels">The matching linked levels if valid</param>
        /// <returns>True if the levels are found. False if any not found</returns>
        bool ViewLinkedProgressiveLevels(IEnumerable<string> levelNames, out IReadOnlyCollection<IViewableLinkedProgressiveLevel> levels);

        /// <summary>
        ///    Transitions linked level claim status from claimed to awarded in one scoped transaction.
        ///    This call must be done on any level that can be awarded
        /// </summary>
        void ClaimAndAwardLinkedProgressiveLevel(string levelName, long winAmount = -1);

        /// <summary>
        ///     Retrieves the list of all linked progressive levels
        /// </summary>
        /// <returns></returns>
        IEnumerable<LinkedProgressiveLevel> GetLinkedProgressiveLevels();
    }
}
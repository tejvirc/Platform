namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using System.Collections.Generic;

    public interface IProgressiveLevelManager
    {
        /// <summary>
        ///     Gets the specified simple meters for the specified progressive device
        /// </summary>
        /// <param name="deviceId">
        ///     The identifier of the progressive device
        /// </param>
        /// <param name="includedMeters">
        ///     The array of meter names
        /// </param>
        IEnumerable<simpleMeter> GetProgressiveLevelMeters(int deviceId, params string[] includedMeters);

        /// <summary>
        /// Updates the specified LinkedProgressiveLevel to use the new valueInCents
        /// </summary>
        /// <param name="progId">The Id for the progressive that will be updated.</param>
        /// <param name="levelId">The Id for the level that will be updated.</param>
        /// <param name="gameId">The Id for the game that will be updated.</param>
        /// <param name="protocolLevelId">The Protocol provided level Id for the level that will be updated</param>
        /// <param name="valueInCents">The new value in cents for the progressive level.</param>
        /// <returns></returns>
        LinkedProgressiveLevel UpdateLinkedProgressiveLevels(int progId, int levelId, int gameId, int protocolLevelId, long valueInCents, bool initialize = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexLevelIds">Dictionary with Monaco and Vertex level Ids mapping</param>
        /// <param name="gameId">Game Id to get value from the dictionary</param>
        /// <param name="progressiveId">Progressive Id to get value from the dictionary</param>
        /// <param name="levelId">Monaco level Id to get value from the dictionary</param>
        /// <returns></returns>
        int GetVertexProgressiveLevelId(
            Dictionary<string, int> vertexLevelIds,
            int gameId,
            int progressiveId,
            int levelId);
    }
}

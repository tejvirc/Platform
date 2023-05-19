namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using System.Collections.Generic;

    public interface IProgressiveLevelManager
    {
        /// <summary>
        ///     Used to convert level ids between internal Monaco ids and external Vertex ids
        /// </summary>
        ProgressiveLevelIdManager LevelIds { get; }

        /// <summary>
        /// The array of the last ProgressiveValues assigned in the SetProgressiveValue command.
        /// The key is stored as "ProgressiveID|LevelID"
        /// </summary>
        Dictionary<string, ProgressiveValue> ProgressiveValues { get; set; }

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
        /// <param name="valueInCents">The new value in cents for the progressive level.</param>
        /// <returns></returns>
        LinkedProgressiveLevel UpdateLinkedProgressiveLevels(int progId, int levelId, long valueInCents, bool initialize = false);
    }
}

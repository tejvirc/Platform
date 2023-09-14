namespace Aristocrat.Monaco.G2S.Services
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Progressives.Linked;

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
        IEnumerable<simpleMeter> GetProgressiveDeviceMeters(int deviceId, params string[] includedMeters);

        /// <summary>
        ///     Gets the specified simple meters for the specified progressive device
        /// </summary>
        /// <param name="linkedLevelName">
        ///     The identifier of the linked progressive level
        /// </param>
        /// <param name="includedMeters">
        ///     The array of meter names
        /// </param>
        IEnumerable<simpleMeter> GetProgressiveLevelMeters(string linkedLevelName, params string[] includedMeters);

        /// <summary>
        /// Updates the specified LinkedProgressiveLevel to use the new valueInCents
        /// </summary>
        /// <param name="progId">The Id for the progressive that will be updated.</param>
        /// <param name="levelId">The Id for the level that will be updated.</param>
        /// <param name="valueInCents">The new value in cents for the progressive level.</param>
        /// <param name="progValueSequence">The sequence number of the most recent progressive value update</param>
        /// <param name="progValueText">A textual description of a progressive prize (such as a car or vacation), empty string if none</param>
        /// <param name="initialize">whether to limit to creation. True means create only, no update. false will create or update as appropriate</param>
        /// <returns></returns>
        LinkedProgressiveLevel UpdateLinkedProgressiveLevels(int progId, int levelId, long valueInCents, long progValueSequence, string progValueText, bool initialize = false);
    }
}

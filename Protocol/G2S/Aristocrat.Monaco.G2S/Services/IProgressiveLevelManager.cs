namespace Aristocrat.Monaco.G2S.Services
{
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using System.Collections.Generic;

    public interface IProgressiveLevelManager
    {
        /// <summary>
        ///     Gets the specified simple meters for the specified progressive device
        /// </summary>
        /// <param name="levelDeviceId">
        ///     The identifier of the progressive level
        /// </param>
        /// <param name="includedMeters">
        ///     The array of meter names
        /// </param>
        IEnumerable<simpleMeter> GetProgressiveLevelMeters(int levelDeviceId, params string[] includedMeters);

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

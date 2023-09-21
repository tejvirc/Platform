namespace Aristocrat.Monaco.G2S.Services
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;

    public interface IProgressiveLevelManager
    {
        /// <summary>
        ///     Gets the specified simple meters for the specified progressive device
        /// </summary>
        /// <param name="progDeviceId">
        ///     The identifier of the progressive device
        /// </param>
        IEnumerable<deviceMeters> GetProgressiveDeviceMeters(int progDeviceId);

        /// <summary>
        /// Updates the specified LinkedProgressiveLevel to use the new valueInCents
        /// </summary>
        /// <param name="progId">The Id for the progressive that will be updated.</param>
        /// <param name="levelId">The Id for the level that will be updated.</param>
        /// <param name="valueInCents">The new value in cents for the progressive level.</param>
        /// <param name="progValueSequence">The sequence number of the most recent progressive value update</param>
        /// <param name="progValueText">A textual description of a progressive prize (such as a car or vacation), empty string if none</param>
        /// <param name="flavorType">The flavor type as specified in progressives.xml. All levels mapped to the linked level must use the same flavor.</param>
        /// <param name="initialize">whether to limit to creation. True means create only, no update. false will create or update as appropriate</param>
        /// <param name="commonLevelName">The common level name to create the level with. Required with initialize is true</param>
        /// <returns></returns>
        LinkedProgressiveLevel UpdateLinkedProgressiveLevels(int progId, int levelId, long valueInCents, long progValueSequence, string progValueText, FlavorType flavorType, bool initialize = false);
    }
}

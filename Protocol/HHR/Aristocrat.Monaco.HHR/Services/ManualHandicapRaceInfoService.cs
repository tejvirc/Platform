namespace Aristocrat.Monaco.Hhr.Services
{
    using Client.Data;

    /// <summary>
    /// </summary>
    public class ManualHandicapRaceInfoService : IManualHandicapRaceInfoService
    {
        /// <inheritdoc />
        public CRaceInfo HandicapRaceInfo => new CRaceInfo();
    }
}
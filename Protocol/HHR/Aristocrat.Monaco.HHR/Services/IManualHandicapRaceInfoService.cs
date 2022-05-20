namespace Aristocrat.Monaco.Hhr.Services
{
    using Client.Data;

    /// <summary>
    ///  Interface to provide information about manual handicap selection
    /// </summary>
    public interface IManualHandicapRaceInfoService
    {
        /// <summary>
        ///     Manual handicap race info
        /// </summary>
        CRaceInfo HandicapRaceInfo { get; }
    }
}

namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using Common;

    /// <summary>
    ///     Provides methods to support reporting EGM Status to the bingo server
    /// </summary>
    public interface IEgmStatusService
    {
        /// <summary>
        ///     Collects required information and returns the EGM status value
        /// </summary>
        EgmStatusFlag GetCurrentEgmStatus();
    }
}
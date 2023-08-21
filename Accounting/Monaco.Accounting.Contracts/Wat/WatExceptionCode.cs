namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{

    /// <summary>
    ///     The Exception codes for WAT transaction
    /// </summary>
    public enum WatExceptionCode
    {
        /// <summary>
        ///     The Exception code used when there is no WAT exception that occurred
        /// </summary>
        None = 0,
        /// <summary>
        ///     The Exception code used when the WAT transaction was cancelled as a result of a power failure
        /// </summary>
        PowerFailure = 6,
        /// <summary>
        ///     The Exception code used when an unknown error happened during a WAT transaction
        /// </summary>
        Other = 99
    }
}
namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    /// <summary>
    ///     Defines the direction of the transfer
    /// </summary>
    public enum WatDirection
    {
        /// <summary>
        ///     Transfer from host to EGM
        /// </summary>
        HostInitiated,

        /// <summary>
        ///     Transfer from EGM to host
        /// </summary>
        EgmInitiated
    }
}
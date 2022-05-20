namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    /// <summary>
    ///     Definitions of WAT trasaction states
    /// </summary>
    public enum WatStatus
    {
        /// <summary>
        ///     Transfer request received from host
        /// </summary>
        RequestReceived,

        /// <summary>
        ///     Cancel request received from host
        /// </summary>
        CancelReceived,

        /// <summary>
        ///     Transfer initiated; waiting for authorization.
        /// </summary>
        Initiated,

        /// <summary>
        ///     Transfer authorized; transfer in process
        /// </summary>
        Authorized,

        /// <summary>
        ///     Transfer action complete/aborted; waiting for ack.
        /// </summary>
        Committed,

        /// <summary>
        ///     Transfer action complete/aborted and acknowledged.
        /// </summary>
        Complete,

        /// <summary>
        ///     Transfer rejected.
        /// </summary>
        Rejected
    }
}
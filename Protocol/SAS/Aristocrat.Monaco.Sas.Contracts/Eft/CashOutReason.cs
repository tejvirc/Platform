namespace Aristocrat.Monaco.Sas.EftTransferProvider
{
    /// <summary>
    ///     Enums for the EftOffProvider to determine what was the cause of the cashout.
    /// </summary>
    public enum CashOutReason
    {
        /// <summary>
        ///     This cashout was not EGM initiated or from cashout accepted.
        /// </summary>
        None,

        /// <summary>
        ///     Cashout was Handled by the timer expiring, it is egm initiated.
        /// </summary>
        TimedOut,

        /// <summary>
        ///     Cashout was Handled by CanCashoutNormally() being called, cashout through host.
        /// </summary>
        CashOutAccepted
    }
}
namespace Aristocrat.Monaco.Accounting.Contracts.TransferOut
{
    /// <summary>
    ///     Defines the reason for transfer out
    /// </summary>
    public enum TransferOutReason
    {
        /// <summary>
        ///     Cash out
        /// </summary>
        CashOut,

        /// <summary>
        ///     Large win
        /// </summary>
        LargeWin,

        /// <summary>
        ///     Bonus pay
        /// </summary>
        BonusPay,

        /// <summary>
        ///     Cash win
        /// </summary>
        CashWin
    }

    /// <summary>
    ///     Transfer Out Reason Extension
    /// </summary>
    public static class TransferOutReasonExtension
    {
        /// <summary>
        ///     Check if the transfer out affects the balance.
        /// </summary>
        /// <param name="reason">Transfer Out Reason</param>
        /// <returns>If the reason affects the balance.</returns>
        public static bool AffectsBalance(this TransferOutReason reason)
        {
            return reason != TransferOutReason.BonusPay &&
                   reason != TransferOutReason.LargeWin &&
                   reason != TransferOutReason.CashWin;
        }
    }
}
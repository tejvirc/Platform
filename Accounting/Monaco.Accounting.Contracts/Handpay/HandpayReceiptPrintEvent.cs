namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    /// <summary>
    ///     An event emitted when requesting a print of a handpay receipt.
    /// </summary>
    public class HandpayReceiptPrintEvent : BaseHandpayEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayReceiptPrintEvent" /> class.
        /// </summary>
        /// <param name="transaction">the ITransaction of the voucher being reprinted</param>
        public HandpayReceiptPrintEvent(HandpayTransaction transaction)
            : base(transaction)
        {
        }
    }
}

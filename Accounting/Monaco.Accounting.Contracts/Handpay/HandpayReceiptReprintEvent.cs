namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    /// <summary>
    ///     Definition of the HandpayReceiptReprintEvent class
    ///     An event emitted when requesting a reprint of a handpay cancelled credit
    /// </summary>
    public class HandpayReceiptReprintEvent : BaseHandpayEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayReceiptReprintEvent" /> class.
        /// </summary>
        /// <param name="transaction">the ITransaction of the voucher being reprinted</param>
        public HandpayReceiptReprintEvent(HandpayTransaction transaction)
            : base(transaction)
        {
        }
    }
}
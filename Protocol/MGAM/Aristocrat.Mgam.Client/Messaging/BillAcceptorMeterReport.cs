namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT to the site controller when the cash can is
    ///     pulled from the VLT.  This is onlu used during Drop Mode.
    /// </summary>
    public class BillAcceptorMeterReport : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets the total value of all currency and vouchers in the bill acceptor.
        /// </summary>
        public int CashBox;

        /// <summary>
        ///     Gets the number of one dollar bills inserted into the bill acceptor.
        /// </summary>
        public int CashBoxOnes;

        /// <summary>
        ///     Gets the number of two dollar bills inserted into the bill acceptor.
        /// </summary>
        public int CashBoxTwos;

        /// <summary>
        ///     Gets the number of five dollar bills inserted into the bill acceptor.
        /// </summary>
        public int CashBoxFives;

        /// <summary>
        ///     Gets the number of ten dollar bills inserted into the bill acceptor.
        /// </summary>
        public int CashBoxTens;

        /// <summary>
        ///     Gets the number of twenty dollar bills inserted into the bill acceptor.
        /// </summary>
        public int CashBoxTwenties;

        /// <summary>
        ///     Gets the number of fifty dollar bills inserted into the bill acceptor.
        /// </summary>
        public int CashBoxFifties;

        /// <summary>
        ///     Gets the number of hundred dollar bills inserted into the bill acceptor.
        /// </summary>
        public int CashBoxHundreds;

        /// <summary>
        ///     Gets the number of vouchers inserted into the bill acceptor.
        /// </summary>
        public int CashBoxVouchers;

        /// <summary>
        ///     Gets the value of all vouchers inserted into the bill acceptor.
        /// </summary>
        public int CashBoxVouchersTotal;
    }
}

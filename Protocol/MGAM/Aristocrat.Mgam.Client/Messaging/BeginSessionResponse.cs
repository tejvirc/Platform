namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent in response to a <see cref="BeginSession"/> message.
    /// </summary>
    public class BeginSessionResponse : Response
    {
        /// <summary>
        ///     Gets or sets the SessionID.
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        ///     Gets or sets SessionCashBalance.
        /// </summary>
        public int SessionCashBalance { get; set; }

        /// <summary>
        ///     Gets or sets SessionCouponBalance.
        /// </summary>
        public int SessionCouponBalance { get; set; }

        /// <summary>
        ///     Gets or sets OffLineVoucherBarcode.
        /// </summary>
        public string OffLineVoucherBarcode { get; set; }

    }
}

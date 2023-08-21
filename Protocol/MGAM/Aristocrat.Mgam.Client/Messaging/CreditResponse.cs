namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent in response to a <see cref="CreditCash"/> message.
    /// </summary>
    public class CreditResponse : Response
    {
        /// <summary>
        ///     Gets or sets the SessionCashBalance.
        /// </summary>
        public int SessionCashBalance { get; set; }

        /// <summary>
        ///     Gets or sets SessionCouponBalance.
        /// </summary>
        public int SessionCouponBalance { get; set; }

        /// <summary>
        ///     Gets or sets ServerTransactionId.
        /// </summary>
        public long ServerTransactionId { get; set; }
    }
}

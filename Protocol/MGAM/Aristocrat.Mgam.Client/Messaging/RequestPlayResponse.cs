namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent in response to a <see cref="RequestPlay"/> message.
    /// </summary>
    public class RequestPlayResponse : Response
    {
        /// <summary>
        ///     Index of the awarded prize.
        /// </summary>
        public int PrizeIndex { get; set; }

        /// <summary>
        ///     Value, in cents, of the ticket.
        /// </summary>
        public long PrizeValue { get; set; }

        /// <summary>
        ///    Additional vendor-specific proprietary information.  This value can be used to add vendor-defined data to the gameset. 
        /// </summary>
        public int ExtendedInfo { get; set; }

        /// <summary>
        ///    Current cash amount, in cents, of current session  
        /// </summary>
        public int SessionCashBalance { get; set; }

        /// <summary>
        ///    Current coupon amount, in cents, of current session.
        /// </summary>
        public int SessionCouponBalance { get; set; }

        /// <summary>
        ///     Signals whether or not this play is a progressive win 
        /// </summary>
        public bool IsProgressiveWin { get; set; }

        /// <summary>
        ///     A unique 64-bit identifier for this RequestPlay transaction that can be used by devices to track prize awards 
        /// </summary>
        public long ServerTransactionId { get; set; }

        /// <summary>
        ///    If IsProgressiveWin is true, this is the value of the progressive jackpot to be added to the PrizeValue to get the total value of the win. 
        /// </summary>
        public long ProgressivePrizeValue { get; set; }
    }
}

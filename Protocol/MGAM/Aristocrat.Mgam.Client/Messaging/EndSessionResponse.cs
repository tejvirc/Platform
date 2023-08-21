namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent in response to a <see cref="EndSession"/> message.
    /// </summary>
    public class EndSessionResponse : Response
    {
        /// <summary>
        ///     Encoded barcode value for the voucher
        /// </summary>
        public string VoucherBarcode { get; set; }

        /// <summary>
        ///     Name of the casino that honors the voucher 
        /// </summary>
        public string CasinoName { get; set; }

        /// <summary>
        ///     Address of casino that honors the voucher 
        /// </summary>
        public string CasinoAddress { get; set; }

        /// <summary>
        ///     Either, “Cash”, “Coupon”, “Cash/Coupon”, or “Limited” 
        /// </summary>
        public string VoucherType { get; set; }

        /// <summary>
        ///     Cash value of the voucher, i.e. “$10,035.50” 
        /// </summary>
        public string CashAmount { get; set; }

        /// <summary>
        ///     Coupon value of the voucher, i.e. “$500.00” 
        /// </summary>
        public string CouponAmount { get; set; }

        /// <summary>
        ///     Total value of the voucher including cash and coupon value 
        /// </summary>
        public string TotalAmount { get; set; }

        /// <summary>
        ///     Long text display of voucher value
        /// </summary>
        public string AmountLongForm { get; set; }

        /// <summary>
        ///     Current datestamp, i.e. “SEP 12, 2003” 
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        ///     Current timestamp, i.e. “13:16:2003” 
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        ///     i.e. “Ticket Void After 30 Days” 
        /// </summary>
        public string Expiration { get; set; }

        /// <summary>
        ///     “12345 678 9”, (DeviceID, sessionID, siteID) 
        /// </summary>
        public string Id { get; set; }
    }
}

namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent in response to a <see cref="ValidateVoucher"/> message.
    /// </summary>
    public class ValidateVoucherResponse : Response
    {
        /// <summary>
        ///     Gets or sets the VoucherCashValue.
        /// </summary>
        public int VoucherCashValue { get; set; }

        /// <summary>
        ///     Gets or sets VoucherCouponValue.
        /// </summary>
        public int VoucherCouponValue { get; set; }
    }
}

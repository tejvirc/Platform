namespace Aristocrat.Monaco.Mgam.Commands
{
    /// <summary>
    ///     Command for ValidateVoucher with a VLT service on the site controller.
    /// </summary>
    public class ValidateVoucher
    {
        /// <summary>
        ///     Gets or sets the Barcode.
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        ///     Gets or sets VoucherCashValue.
        /// </summary>
        public int VoucherCashValue { get; set; }

        /// <summary>
        ///     Gets or sets the VoucherCouponValue.
        /// </summary>
        public int VoucherCouponValue { get; set; }
    }
}

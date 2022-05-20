namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     VoucherOutReason
    /// </summary>
    public enum VoucherOutOfflineReason
    {
        /// <summary>
        ///     Reason None.
        /// </summary>
        None,

        /// <summary>
        ///     Reason Cashout.
        /// </summary>
        Cashout,

        /// <summary>
        ///     Reason request play.
        /// </summary>
        RequestPlay,

        /// <summary>
        ///     Reason credit.
        /// </summary>
        Credit
    }

    /// <summary>
    ///     Stores data about the voucher.
    /// </summary>
    public class Voucher : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the voucher barcode.
        /// </summary>
        public string VoucherBarcode { get; set; }

        /// <summary>
        ///     Gets or sets the CasinoName.
        /// </summary>
        public string CasinoName { get; set; }

        /// <summary>
        ///     Gets or sets the CasinoAddress.
        /// </summary>
        public string CasinoAddress { get; set; }

        /// <summary>
        ///     Gets or sets the CashAmount.
        /// </summary>
        public string VoucherType { get; set; }

        /// <summary>
        ///     Gets or sets the CashAmount.
        /// </summary>
        public string CashAmount { get; set; }

        /// <summary>
        ///     Gets or sets the CouponAmount.
        /// </summary>
        public string CouponAmount { get; set; }

        /// <summary>
        ///     Gets or sets the TotalAmount.
        /// </summary>
        public string TotalAmount { get; set; }

        /// <summary>
        ///     Gets or sets the AmountLongForm.
        /// </summary>
        public string AmountLongForm { get; set; }

        /// <summary>
        ///     Gets or sets the Date.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        ///     Gets or sets the Time.
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        ///     Gets or sets the Expiration.
        /// </summary>
        public string Expiration { get; set; }

        /// <summary>
        ///     Gets or sets the Device Id.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the voucher out reason.
        /// </summary>
        public VoucherOutOfflineReason OfflineReason { get; set; }
    }
}

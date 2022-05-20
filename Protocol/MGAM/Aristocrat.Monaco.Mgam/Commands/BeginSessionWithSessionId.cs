namespace Aristocrat.Monaco.Mgam.Commands
{
    /// <summary>
    ///     Command for begin session with session Id with a VLT service on the site controller.
    /// </summary>
    public class BeginSessionWithSessionId
    {
        /// <summary>
        ///     Gets or sets the session Id.
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        ///     Gets or sets the voucher printed offline flag.
        /// </summary>
        public bool VoucherPrintedOffline { get; set; }

        /// <summary>
        ///     Gets or sets the barcode.
        /// </summary>
        public string Barcode { get; set; }
    }
}

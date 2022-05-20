namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Stores data about the session with the VLT service.
    /// </summary>
    public class Session : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the SessionId returned in the BeginSessionResponse message.
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        ///     Gets or sets the OfflineVoucherBarcode returned in the BeginSessionResponse message.
        /// </summary>
        public string OfflineVoucherBarcode { get; set; }

        /// <summary>
        ///     Gets or sets the OfflineVoucherPrinted used with BeginSessionWithSessionId.
        /// </summary>
        public bool OfflineVoucherPrinted { get; set; }
    }
}

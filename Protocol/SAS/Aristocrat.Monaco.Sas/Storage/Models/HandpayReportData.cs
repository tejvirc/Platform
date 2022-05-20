namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using Common.Storage;

    /// <summary>
    ///     The handpay report data entity
    /// </summary>
    public class HandpayReportData : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the client id
        /// </summary>
        public byte ClientId { get; set; }

        /// <summary>
        ///     Gets or sets the queue
        /// </summary>
        public string Queue { get; set; }
    }
}
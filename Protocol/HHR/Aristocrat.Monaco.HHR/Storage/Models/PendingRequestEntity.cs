namespace Aristocrat.Monaco.Hhr.Storage.Models
{
    using Common.Storage;

    /// <summary>
    ///     Model for the PendingRequestEntity messages.
    /// </summary>
    public class PendingRequestEntity : BaseEntity
    {
        /// <summary>
        ///     String representing pending IEnumerable<!--(Request, Type) -->
        /// </summary>
        public string PendingRequests { get; set; }
    }
}
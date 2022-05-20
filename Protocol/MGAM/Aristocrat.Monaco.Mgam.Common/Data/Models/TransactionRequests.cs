namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Stores data about the Transaction Requests.
    /// </summary>
    public class TransactionRequests : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the Transaction Requests awards.
        /// </summary>
        public string Requests { get; set; }
    }
}

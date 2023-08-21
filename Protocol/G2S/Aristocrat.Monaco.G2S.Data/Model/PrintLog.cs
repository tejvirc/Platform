namespace Aristocrat.Monaco.G2S.Data.Model
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     Print Log Entity
    /// </summary>
    public class PrintLog : BaseEntity, ILogSequence
    {
        /// <summary>
        ///     Gets or sets the unique transaction identifier
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the device identifier of the printer that generated the transaction
        /// </summary>
        public int PrinterId { get; set; }

        /// <summary>
        ///     Gets or sets the Date/time that the printer action took place
        /// </summary>
        public DateTime PrintDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the template index associated with this transaction
        /// </summary>
        public int TemplateIndex { get; set; }

        /// <summary>
        ///     Gets or sets the transfer status code; see GDS protocol; 1 (one) implies that the transfer was successful
        /// </summary>
        public int State { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the entire print request was completed
        /// </summary>
        public bool Complete { get; set; }
    }
}
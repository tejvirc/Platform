namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Aristocrat.Sas.Client;
    using Common.Storage;

    /// <summary>
    ///     The entity for validation information
    /// </summary>
    public class ValidationInformation : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets the extended ticket data status
        /// </summary>
        public TicketDataStatus ExtendedTicketDataStatus { get; set; }

        /// <summary>
        ///     Gets or sets whether or not extended ticket data is being used
        /// </summary>
        public bool ExtendedTicketDataSet { get; set; }

        /// <summary>
        ///     Gets or sets the last received sequence number
        /// </summary>
        public long LastReceivedSequenceNumber { get; set; }

        /// <summary>
        ///     Gets or sets the current sequence number
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        ///     Gets or sets the machine validation id
        /// </summary>
        public long MachineValidationId { get; set; }

        /// <summary>
        ///     Gets or sets whether or not validation has been configured
        /// </summary>
        public bool ValidationConfigured { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

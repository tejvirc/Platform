namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;

    /// <inheritdoc />
    public class SendEnhancedValidationInformation : LongPollData
    {
        /// <summary>
        ///     Gets or sets the function code
        /// </summary>
        public int FunctionCode { get; set; }
    }

    /// <inheritdoc />
    public class SendEnhancedValidationInformationResponse : LongPollResponse
    {
        /// <summary>
        ///     Gets or sets the validation type
        /// </summary>
        public int ValidationType { get; set; }

        /// <summary>
        ///     Gets or sets the index number
        /// </summary>
        public long Index { get; set; }

        /// <summary>
        ///     Gets or sets the validation date
        /// </summary>
        public DateTime ValidationDate { get; set; }

        /// <summary>
        ///     Gets or sets the validation number
        /// </summary>
        public ulong ValidationNumber { get; set; }

        /// <summary>
        ///     Gets or sets the amount
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets or sets the ticket number
        /// </summary>
        public long TicketNumber { get; set; }

        /// <summary>
        ///     Gets or sets the validation system id
        /// </summary>
        public ulong ValidationSystemId { get; set; }

        /// <summary>
        ///     Gets or sets the expiration date
        /// </summary>
        public uint ExpirationDate { get; set; }

        /// <summary>
        ///     Gets or sets the pool id
        /// </summary>
        public ushort PoolId { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the response was successful
        /// </summary>
        public bool Successful { get; set; }
    }
}
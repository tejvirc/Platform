namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using Aristocrat.Sas.Client;

    /// <summary>
    /// Represents the ticket type for ticket outs.
    /// </summary>
    public enum TicketType
    {
        /// <summary>
        /// No type specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Voided ticket.
        /// </summary>
        Void,

        /// <summary>
        /// Cash out ticket.
        /// </summary>
        CashOut,

        /// <summary>
        /// Forced cashout receipt, not validated.
        /// </summary>
        CashOutOffline,

        /// <summary>
        /// Validated cash out receipt.
        /// </summary>
        CashOutReceipt,

        /// <summary>
        /// Jackpot ticket.
        /// </summary>
        Jackpot,

        /// <summary>
        /// Forced jackpot receipt.  Not validated.
        /// </summary>
        JackpotOffline,

        /// <summary>
        /// Validated jackpot receipt.
        /// </summary>
        JackpotReceipt,

        /// <summary>
        /// Validated handpay.
        /// </summary>
        HandPayValidated,

        /// <summary>
        /// Restricted ticket.
        /// </summary>
        Restricted,

        /// <summary>
        /// Max number of ticket types.
        /// </summary>
        MaxTicketTypes
    }

    /// <summary>
    /// Definition of the TicketOutInfo interface.
    /// </summary>
    public class TicketOutInfo
    {
        /// <summary>
        /// Gets or sets the validation type
        /// </summary>
        public TicketValidationType ValidationType { get; set; }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        public ulong Amount { get; set; }

        /// <summary>
        /// Gets or sets the barcode
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// Gets or sets the time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets the ticket expiration
        /// </summary>
        public uint TicketExpiration { get; set; }

        /// <summary>
        /// Gets or sets the pool id.
        /// </summary>
        public ushort Pool { get; set; }
    }
}

namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     The extend ticket data codes used for setting ticket data
    /// </summary>
    public enum ExtendTicketDataCode
    {
        Location = 0,
        Address1,
        Address2,
        RestrictedTicketTitle = 0x10,
        DebitTicketTitle = 0x20
    }

    /// <inheritdoc />
    public class SetTicketData : LongPollData
    {
        /// <summary>
        ///     Gets or sets the Host ID
        /// </summary>
        public int HostId { get; set; }

        /// <summary>
        /// Gets or sets the Expiration Date
        /// </summary>
        public int ExpirationDate { get; set; }

        /// <summary>
        ///     Gets or sets the location ticket data
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        ///     Gets or sets the address1 ticket data
        /// </summary>
        public string Address1 { get; set; }

        /// <summary>
        ///     Gets or sets the address2 ticket data
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        ///     Gets or sets the restricted ticket title
        /// </summary>
        public string RestrictedTicketTitle { get; set; }

        /// <summary>
        ///     Gets or sets the debit ticket title
        /// </summary>
        public string DebitTicketTitle { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the ticket data is valid
        /// </summary>
        public bool ValidTicketData { get; set; } = true;

        /// <summary>
        ///     Gets or sets whether or not the ticket data being processed in for setting extended data or not
        /// </summary>
        public bool IsExtendTicketData { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this is a broadcast poll
        /// </summary>
        public bool BroadcastPoll { get; set; }

        /// <summary>
        ///     Gets or sets whether or not we should set the expiration date
        /// </summary>
        public bool SetExpirationDate { get; set; }
    }
}
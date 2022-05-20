namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    ///     Definition of the TicketData class.
    /// </summary>
    public class TicketData
    {
        /// <summary>
        ///     Initializes a new instance of the TicketData class.
        /// </summary>
        public TicketData()
        {
            Location = null;
            Address1 = null;
            Address2 = null;
            RestrictedTicketTitle = null;
        }

        /// <summary>
        ///     Gets or sets the location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        ///     Gets or sets the address line 1
        /// </summary>
        public string Address1 { get; set; }

        /// <summary>
        ///     Gets or sets the address line 2
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        ///     Gets or sets the restricted ticket title data on the ticket to be printed.
        /// </summary>
        public string RestrictedTicketTitle { get; set; }

        /// <summary>
        ///     Gets or sets the debit ticket title data on the ticket to be printed.
        /// </summary>
        public string DebitTicketTitle { get; set; }
    }
}

namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    /// Definition of the ITicketDataProvider interface.
    /// </summary>
    public interface ITicketDataProvider
    {
        /// <summary>
        ///     Gets the ticket data sent by Sas.
        /// </summary>
        /// <returns>The TicketData object containing the ticket data requested.</returns>
        TicketData TicketData { get; }

        /// <summary>
        ///     Called when Sas host has sent ticket data from the backend.  Data set by LP 7C takes
        ///     precedence over any data set by LP 7D.
        /// </summary>
        /// <param name="newTicketData">Ticket data to be set</param>
        void SetTicketData(TicketData newTicketData);
    }
}

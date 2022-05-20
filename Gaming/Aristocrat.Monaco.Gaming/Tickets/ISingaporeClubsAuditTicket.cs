namespace Aristocrat.Monaco.Gaming.Tickets
{
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Interface for the Singapore Clubs audit ticket
    /// </summary>
    public interface ISingaporeClubsAuditTicket
    {
        /// <summary>
        ///     Create a ticket that reports a specific set of meters
        /// </summary>
        /// <returns>The data to be printed</returns>
        Ticket CreateMainMetersPage();

        /// <summary>
        ///     Create a ticket that reports the last 10 progressive wins
        /// </summary>
        /// <returns>The data to be printed</returns>
        Ticket CreateProgressiveHistoryPage();
    }
}
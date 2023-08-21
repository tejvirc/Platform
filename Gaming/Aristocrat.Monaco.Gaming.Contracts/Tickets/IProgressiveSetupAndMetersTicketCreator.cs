namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IProgressiveSetupAndMetersTicketCreator interface.
    /// </summary>
    public interface IProgressiveSetupAndMetersTicketCreator
    {
        /// <summary>
        ///     Creates tickets containing progressive setup and meter values.  Checks TicketMode.
        /// </summary>
        /// <param name="game">The game for which meters will be included.</param>
        /// <param name="denomMillicents">The denomination for which meters will be included.</param>
        /// <returns>Tickets with fields required for progressive setup and meters.</returns>
        List<Ticket> CreateProgressiveSetupAndMetersTicket(IGameDetail game, long denomMillicents);
    }
}

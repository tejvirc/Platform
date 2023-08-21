namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using System.Collections.Generic;
    using Application.Contracts.MeterPage;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IGameMetersTicketCreator interface.
    /// </summary>
    public interface IGameMetersTicketCreator
    {
        /// <summary>
        ///     Creates tickets containing game meter values.  Checks TicketMode.
        /// </summary>
        /// <param name="gameId">The ID of the game for which meters will be included.</param>
        /// <param name="meterNodes">The list of meter nodes.</param>
        /// <param name="useMasterValues">Indicates whether to use master values (true) or period values (false).</param>
        /// <param name="onlySelected">Indicates whether to only use the selected gameId.</param>
        /// <returns>Tickets with fields required for an EGM meters ticket.</returns>
        List<Ticket> CreateGameMetersTicket(int gameId, IList<MeterNode> meterNodes, bool useMasterValues, bool onlySelected);
    }
}

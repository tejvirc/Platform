namespace Aristocrat.Monaco.Gaming.Contracts.Tickets
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IGameMetersTicketCreator interface.
    /// </summary>
    public interface IGameMetersTicketCreator : IService
    {
        /// <summary>
        ///     Creates tickets containing game meter values.  Checks TicketMode.
        /// </summary>
        /// <param name="game">Game for which meters will be included.</param>
        /// <param name="meters">The list of meters.</param>
        /// <param name="useMasterValues">Indicates whether to use master values (true) or period values (false).</param>
        /// <returns>Tickets with fields required for an EGM meters ticket.</returns>
        List<Ticket> Create(IGameDetail game, IList<Tuple<IMeter, string>> meters, bool useMasterValues);
    }
}

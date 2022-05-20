////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IPeriodicResetTicketCreator.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IPeriodicResetTicketCreator interface.
    /// </summary>
    public interface IPeriodicResetTicketCreator
    {
        /// <summary>
        ///     Creates a ticket containing meter values.
        /// </summary>
        /// <returns>A Ticket object with fields required for a periodic reset ticket.</returns>
        Ticket Create();
    }
}
////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IIdentityTicketCreator.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2016 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
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
    ///     Definition of the IIdentityTicketCreator interface.
    /// </summary>
    public interface IIdentityTicketCreator
    {
        /// <summary>
        ///     Creates an identity ticket.
        /// </summary>
        /// <returns>A Ticket object with fields required for an identity ticket.</returns>
        Ticket CreateIdentityTicket();
    }
}
////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="ISingaporeClubsAuditTicketCreator.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the ISingaporeClubsAuditTicketCreator interface.
    /// </summary>
    public interface ISingaporeClubsAuditTicketCreator
    {
        /// <summary>
        ///     Creates a ticket containing meter values.
        /// </summary>
        /// <returns>A Ticket object with fields required for the Singapore Audit ticket.</returns>
        IEnumerable<Ticket> Create();
    }
}
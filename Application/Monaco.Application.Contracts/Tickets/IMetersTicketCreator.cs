////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IMetersTicketCreator.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2016 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using Kernel;
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IMetersTicketCreator interface.
    /// </summary>
    public interface IMetersTicketCreator : IService
    {
        /// <summary>
        ///     Creates a ticket containing meter values.
        /// </summary>
        /// <param name="meters">
        ///     The ordered collection of meters to put on the ticket where the Item2 string is the meter name to
        ///     print.
        /// </param>
        /// <param name="useMasterValues">Indicates whether to use master values (true) or period values (false).</param>
        /// <returns>A Ticket object with fields required for an EGM meters ticket.</returns>
        Ticket CreateEgmMetersTicket(IList<Tuple<IMeter, string>> meters, bool useMasterValues);

        /// <summary>
        ///     Creates a ticket containing meter values.
        /// </summary>
        /// <param name="meters">
        ///     The ordered collection of meters to put on the ticket where the Item2 string is the meter name to
        ///     print.
        /// </param>
        /// <param name="useMasterValues">Indicates whether to use master values (true) or period values (false).</param>
        /// <returns>A Ticket object with fields required for an EGM meters ticket.</returns>
        Ticket CreateEgmMetersTicket(IList<Tuple<Tuple<IMeter, IMeter>, string>> meters, bool useMasterValues);

        /// <summary>
        ///     Creates tickets containing meter values.  Checks TicketMode.
        /// </summary>
        /// <param name="meters">
        ///     The ordered collection of meters to put on the ticket where the Item2 string is the meter name to
        ///     print.
        /// </param>
        /// <param name="useMasterValues">Indicates whether to use master values (true) or period values (false).</param>
        /// <returns>Tickets with fields required for an EGM meters ticket.</returns>
        List<Ticket> CreateMetersTickets(IList<Tuple<IMeter, string>> meters, bool useMasterValues);
    }
}
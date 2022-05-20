////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IEventLogTicketCreator.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using System.Collections.ObjectModel;
    using Kernel;
    using Hardware.Contracts.Ticket;
    using TiltLogger;

    /// <summary>
    ///     Definition of the IEventLogTicketCreator interface.
    /// </summary>
    public interface IEventLogTicketCreator : IService
    {
        /// <summary>
        ///     Gets the number of events that can be printed per page
        /// </summary>
        int EventsPerPage { get; }

        /// <summary>
        ///     Creates an event log ticket.
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="events">collection of events to be printed</param>
        /// <returns>A Ticket object with fields required for an event log ticket.</returns>
        Ticket Create(int page, Collection<EventDescription> events);
    }
}
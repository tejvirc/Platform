////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="PeriodicResetTicketCreator.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;

    /// <summary>
    ///     This class creates meters ticket objects
    /// </summary>
    public class PeriodicResetTicketCreator : IPeriodicResetTicketCreator, IService
    {
        public Ticket Create()
        {
            var ticket = new PeriodicResetTicket();
            return ticket.CreateTextTicket();
        }

        public Ticket CreateSecondPage()
        {
            var ticket = new PeriodicResetTicket();
            return ticket.CreateSecondPageTextTicket();
        }

        public string Name => "Periodic Reset Ticket Creator";

        public ICollection<Type> ServiceTypes => new[] { typeof(IPeriodicResetTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public virtual void Initialize()
        {
        }
    }
}
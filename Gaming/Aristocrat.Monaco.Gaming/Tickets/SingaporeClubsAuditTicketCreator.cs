namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using log4net;

    /// <summary>
    ///     This class creates meters ticket objects
    /// </summary>
    public class SingaporeClubsAuditTicketCreator : ISingaporeClubsAuditTicketCreator, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Name => "Singapore Clubs Audit Ticket";

        public ICollection<Type> ServiceTypes => new[] { typeof(ISingaporeClubsAuditTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public virtual void Initialize()
        {
        }

        public IEnumerable<Ticket> Create()
        {
            var pages = new List<Ticket>();

            ISingaporeClubsAuditTicket singaporeTicket = new SingaporeClubsAuditTicket();

            pages.Add(singaporeTicket.CreateMainMetersPage());
            pages.Add(singaporeTicket.CreateProgressiveHistoryPage());

            Logger.Info("Printing Singapore Clubs audit tickets");

            return pages;
        }
    }
}
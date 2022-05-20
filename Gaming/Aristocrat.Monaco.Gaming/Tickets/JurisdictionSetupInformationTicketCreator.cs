namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;

    /// <summary>
    ///     This class creates game meters ticket objects
    /// </summary>
    public class JurisdictionSetupInformationTicketCreator : IJurisdictionSetupInformationTicketCreator, IService
    {
        /// <inheritdoc />
        public List<Ticket> CreateJurisdictionSetupInformationTicket()
        {
            var behavior = ServiceManager.GetInstance().GetService<IPropertiesManager>().GetValue(
                ApplicationConstants.TicketModeAuditKey,
                TicketModeAuditBehavior.Audit);

            return behavior == TicketModeAuditBehavior.Inspection ? new JurisdictionSetupInformationTicket().CreateAuditTickets() : null;
        }

        /// <inheritdoc />
        public string Name => "JurisdictionSetup Information Ticket Creator";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IJurisdictionSetupInformationTicketCreator) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}

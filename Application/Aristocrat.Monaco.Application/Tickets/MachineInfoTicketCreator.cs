namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.MarketConfig.Models.Application;

    public class MachineInfoTicketCreator : IMachineInfoTicketCreator, IService
    {
        public List<Ticket> Create()
        {
            switch (ServiceManager.GetInstance().GetService<IPropertiesManager>()?.GetValue(
                    ApplicationConstants.TicketModeAuditKey,
                    TicketModeAuditBehavior.Audit))
            {
                case TicketModeAuditBehavior.Inspection:
                    return new MachineSetupInformationTicket().CreateAuditTickets();
                default:
                    return new MachineInfoTicket().CreateTickets();
            }
        }

        public string Name => "Gaming Machine Info Ticket Creator";

        public ICollection<Type> ServiceTypes => new[] { typeof(IMachineInfoTicketCreator) };

        /// <summary>
        /// </summary>
        public void Initialize()
        {
        }
    }
}
namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.MarketConfig.Models.Application;

    /// <summary>
    ///     This class creates game meters ticket objects
    /// </summary>
    public class ProgressiveSetupAndMetersTicketCreator : IProgressiveSetupAndMetersTicketCreator, IService
    {
        /// <inheritdoc />
        public List<Ticket> CreateProgressiveSetupAndMetersTicket(
            IGameDetail game,
            long denomMillicents)
        {
            if (ServiceManager.GetInstance().GetService<IPropertiesManager>()?.GetValue(
                    ApplicationConstants.TicketModeAuditKey,
                    TicketModeAuditBehavior.Audit) == TicketModeAuditBehavior.Inspection)
            {
                return new ProgressiveSetupAndMetersTicket(game, denomMillicents).CreateAuditTickets();
            }

            return null;
        }

        /// <inheritdoc />
        public string Name => "Progressive Setup and Meters Ticket Creator";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IProgressiveSetupAndMetersTicketCreator) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}

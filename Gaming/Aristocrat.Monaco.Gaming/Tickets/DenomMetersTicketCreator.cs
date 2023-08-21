namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     This class creates game meters ticket objects
    /// </summary>
    public class DenomMetersTicketCreator : IDenomMetersTicketCreator, IService
    {
        /// <inheritdoc />
        public List<Ticket> CreateDenomMetersTicket(long denomMillicents, bool isLifetime)
        {
            if (ServiceManager.GetInstance().GetService<IPropertiesManager>()?.GetValue(
                    ApplicationConstants.TicketModeAuditKey,
                    TicketModeAuditBehavior.Audit) == TicketModeAuditBehavior.Inspection)
            {
                return new DenomPerformanceMetersTicket(denomMillicents).CreateAuditTickets();
            }

            return new DenomPerformanceMetersTicket(
                denomMillicents,
                isLifetime,
                Localizer.For(CultureFor.OperatorTicket).GetString(isLifetime ? ResourceKeys.DenomMetersLifetime : ResourceKeys.DenomMetersPeriod)
                ).CreateAuditTickets();
        }

        /// <inheritdoc />
        public string Name => "Denom Meters Ticket Creator";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IDenomMetersTicketCreator) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}

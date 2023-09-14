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
            var properties = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            if (properties?.GetValue(
                    ApplicationConstants.TicketModeAuditKey,
                    TicketModeAuditBehavior.Audit) == TicketModeAuditBehavior.Inspection)
            {
                return new DenomPerformanceMetersTicket(denomMillicents).CreateAuditTickets();
            }

            var localizer = (properties?.GetValue(ApplicationConstants.LocalizationOperatorTicketLanguageSettingOperatorOverride, false) ?? false)
                ? Localizer.For(CultureFor.Operator)
                : Localizer.For(CultureFor.OperatorTicket);

            return new DenomPerformanceMetersTicket(
                denomMillicents,
                isLifetime,
                localizer.GetString(isLifetime ? ResourceKeys.DenomMetersLifetime : ResourceKeys.DenomMetersPeriod)
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

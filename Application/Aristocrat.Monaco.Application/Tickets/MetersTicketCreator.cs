namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This class creates meters ticket objects
    /// </summary>
    public class MetersTicketCreator : IMetersTicketCreator
    {
        private readonly ILocalizer _localizer = (ServiceManager.GetInstance().TryGetService<IPropertiesManager>()?.GetValue(
            ApplicationConstants.LocalizationOperatorTicketLanguageSettingOperatorOverride, false) ?? false)
            ? Localizer.For(CultureFor.Operator)
            : Localizer.For(CultureFor.OperatorTicket);

        /// <inheritdoc />
        public List<Ticket> CreateMetersTickets(IList<Tuple<IMeter, string>> meters, bool useMasterValues)
        {
            switch (ServiceManager.GetInstance().TryGetService<IPropertiesManager>()?.GetValue(
                ApplicationConstants.TicketModeAuditKey,
                TicketModeAuditBehavior.Audit))
            {
                case TicketModeAuditBehavior.Inspection:
                    return new MainAccountingMetersTicket().CreateAuditTickets();
                default:
                    var title = useMasterValues
                        ? _localizer.GetString(ResourceKeys.MasterMetersTicketTitleText).ToUpper()
                        : _localizer.GetString(ResourceKeys.PeriodMetersTicketTitleText).ToUpper();
                    return new ValueMeterTicket(title, useMasterValues, meters).CreateAuditTickets();
            }
        }

        public Ticket CreateEgmMetersTicket(IList<Tuple<IMeter, string>> meters, bool useMasterValues)
        {
            var title = useMasterValues
                ? _localizer.GetString(ResourceKeys.MasterMetersTicketTitleText).ToUpper()
                : _localizer.GetString(ResourceKeys.PeriodMetersTicketTitleText).ToUpper();
            return new ValueMeterTicket(title, useMasterValues, meters).CreateTextTicket();
        }

        public Ticket CreateEgmMetersTicket(IList<Tuple<Tuple<IMeter, IMeter>, string>> meters, bool useMasterValues)
        {
            var title = useMasterValues
                ? _localizer.GetString(ResourceKeys.MasterMetersTicketTitleText).ToUpper()
                : _localizer.GetString(ResourceKeys.PeriodMetersTicketTitleText).ToUpper();
            return new ValueCountMeterTicket(title, useMasterValues, meters).CreateTextTicket();
        }

        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public string Name => "Meters Ticket Creator";

        /// <summary>
        ///     Gets the interface this service implements and exposes as a service
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IMetersTicketCreator) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public void Initialize()
        {
        }
    }
}
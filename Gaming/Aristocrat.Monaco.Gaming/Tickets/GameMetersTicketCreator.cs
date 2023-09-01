namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using Loc = Application.Contracts.Localization.Localizer;

    /// <summary>
    ///     This class creates game meters ticket objects
    /// </summary>
    public class GameMetersTicketCreator : IGameMetersTicketCreator
    {
        private IServiceManager _serviceManager;
        private IPropertiesManager _propertiesManager;
        private ILocalizer _localizer;

        private ILocalizer Localizer => _localizer ??= (_propertiesManager?
            .GetValue(ApplicationConstants.LocalizationOperatorTicketLanguageSettingOperatorOverride, false) ?? false)
            ? Loc.For(CultureFor.Operator)
            : Loc.For(CultureFor.OperatorTicket);

        /// <inheritdoc />
        public List<Ticket> Create(
            IGameDetail game,
            IList<Tuple<IMeter, string>> meters,
            bool useMasterValues)
        {
            switch (ServiceManager.GetInstance().TryGetService<IPropertiesManager>()?.GetValue(
               ApplicationConstants.TicketModeAuditKey,
               TicketModeAuditBehavior.Audit))
            {
                case TicketModeAuditBehavior.Inspection:
                    return new GamePerformanceMetersTicket(game).CreateAuditTickets();
                default:
                    var title = useMasterValues
                        ? Localizer.GetString(ResourceKeys.MasterGameMetersTicketTitleText).ToUpper(Localizer.CurrentCulture)
                        : Localizer.GetString(ResourceKeys.PeriodGameMetersTicketTitleText).ToUpper(Localizer.CurrentCulture);
                    return new GameValueMeterTicket(title, meters, useMasterValues, game).CreateAuditTickets();
            }
        }

        /// <inheritdoc />
        public string Name => "Game Meters Ticket Creator";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameMetersTicketCreator) };

        /// <inheritdoc />
        public void Initialize()
        {
            _serviceManager = ServiceManager.GetInstance();
            _propertiesManager = _serviceManager.GetService<IPropertiesManager>();
        }
    }
}

namespace Aristocrat.Monaco.Accounting
{
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Contracts;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     A rule that blocks clearing of persisted data unless there are no credits on the EGM.
    /// </summary>
    public class EmptyCreditBalancePersistenceClearRule : BasePersistenceClearRule
    {
        private readonly string _creditsExistDenyReason;

        /// <summary>Initializes a new instance of the <see cref="EmptyCreditBalancePersistenceClearRule" /> class.</summary>
        public EmptyCreditBalancePersistenceClearRule()
        {
            var serviceManager = ServiceManager.GetInstance();

            var eventBus = serviceManager.GetService<IEventBus>();
            eventBus.Subscribe<ServiceAddedEvent>(this, HandleServiceAddedEvent);
            eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleBankBalanceChangedEvent);

            _creditsExistDenyReason =
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CreditsExistDenyReason);

            var creditsUnavailableDenyReason = Localizer.For(CultureFor.OperatorTicket)
                .GetString(ResourceKeys.CreditsUnavailableDenyReason);

            var bank = serviceManager.TryGetService<IBank>();
            if (bank == null)
            {
                SetAllowed(false, false, creditsUnavailableDenyReason);
            }
            else
            {
                eventBus.Unsubscribe<ServiceAddedEvent>(this);
                var allow = bank.QueryBalance() == 0;
                SetAllowed(allow, allow, _creditsExistDenyReason);
            }
        }

        private void HandleServiceAddedEvent(ServiceAddedEvent theEvent)
        {
            if (theEvent.ServiceType == typeof(IBank))
            {
                ServiceManager.GetInstance().GetService<IEventBus>().Unsubscribe<ServiceAddedEvent>(this);
                var allow = ServiceManager.GetInstance().GetService<IBank>().QueryBalance() == 0;
                SetAllowed(allow, allow, _creditsExistDenyReason);
            }
        }

        private void HandleBankBalanceChangedEvent(BankBalanceChangedEvent theEvent)
        {
            var allow = theEvent.NewBalance == 0;
            SetAllowed(allow, allow, _creditsExistDenyReason);
        }
    }
}
namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Accounting.Contracts;
    using Commands;
    using Contracts;
    using Contracts.Barkeeper;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="CurrencyInCompletedEvent" />.
    ///     This class internationally inherits from Kernel.Consumes to have its own subscription context
    /// </summary>
    public class CurrencyInConsumer : Kernel.Consumes<CurrencyInCompletedEvent>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IPlayerBank _playerBank;
        private readonly IBarkeeperHandler _barkeeperHandler;
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IPersistentStorageManager _persistentStorage;

        public CurrencyInConsumer(
            IEventBus eventBus,
            ICommandHandlerFactory commandFactory,
            IPropertiesManager properties,
            IPlayerBank playerBank,
            IBarkeeperHandler barkeeperHandler,
            ICurrencyInContainer currencyHandler,
            ISessionInfoService sessionInfoService,
            IPersistentStorageManager persistentStorage)
            : base(eventBus, null, evt => evt.Amount > 0)
        {
            _propertiesManager = properties ?? throw new ArgumentNullException(nameof(properties));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
            _currencyHandler = currencyHandler ?? throw new ArgumentNullException(nameof(currencyHandler));
            _sessionInfoService = sessionInfoService ?? throw new ArgumentNullException(nameof(sessionInfoService));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        public override void Consume(CurrencyInCompletedEvent theEvent)
        {
            var maxCreditMeter = _propertiesManager.GetValue(
                AccountingConstants.MaxCreditMeter,
                0L);

            var allowCreditsInAboveMaxCredit = _propertiesManager.GetValue(
                AccountingConstants.AllowCreditsInAboveMaxCredit,
                false);

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                // Cash out as soon as bank goes beyond maxCreditMeter and credits in is allowed above max credit limit.
                if (allowCreditsInAboveMaxCredit && _playerBank.Balance >= maxCreditMeter)
                {
                    _commandFactory.Create<CheckBalance>().Handle(new CheckBalance());
                }

                if (theEvent.Transaction != null)
                {
                    _currencyHandler.Credit(theEvent.Transaction);
                    _sessionInfoService.HandleTransaction(theEvent.Transaction);
                }

                _barkeeperHandler.OnCreditsInserted(theEvent.Amount);
                scope.Complete();
            }
        }
    }
}

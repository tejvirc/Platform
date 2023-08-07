namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.MarketConfig.Models.Accounting;

    public class HandpayKeyedOffConsumer : Consumes<HandpayKeyedOffEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _history;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IPersistentStorageManager _persistentStorage;

        private readonly bool _meterFreeGames;

        /// <summary>
        ///     Initializes a new instance of the HandpayKeyedOffConsumer class.
        /// </summary>
        /// <param name="currencyHandler">The currency in container.</param>
        /// <param name="gameHistory">The game history provider.</param>
        /// <param name="history">The history provider.</param>
        /// <param name="properties">The properties provider.</param>
        /// <param name="sessionInfoService">The session information provider.</param>
        /// <param name="persistentStorage">The persistence storage manager.</param>
        public HandpayKeyedOffConsumer(
            ICurrencyInContainer currencyHandler,
            IGameHistory gameHistory,
            ITransactionHistory history,
            IPropertiesManager properties,
            ISessionInfoService sessionInfoService,
            IPersistentStorageManager persistentStorage)
        {
            _currencyHandler = currencyHandler ?? throw new ArgumentNullException(nameof(currencyHandler));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _sessionInfoService = sessionInfoService ?? throw new ArgumentNullException(nameof(sessionInfoService));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            _meterFreeGames = properties.GetValue(GamingConstants.MeterFreeGamesIndependently, false);
        }

        /// <summary>
        ///     Handles the event.
        /// </summary>
        /// <param name="event">The HandpayKeyedOffEvent to handle.</param>
        public override void Consume(HandpayKeyedOffEvent @event)
        {
            if (@event?.Transaction == null)
            {
                return;
            }

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                switch (@event.Transaction.KeyOffType)
                {
                    case KeyOffType.LocalHandpay:
                    case KeyOffType.LocalCredit:
                    case KeyOffType.RemoteHandpay:
                    case KeyOffType.RemoteCredit:
                        @event.Transaction.HandleOutTransaction(
                            @event.Transaction.TransactionAmount,
                            _currencyHandler,
                            _gameHistory,
                            _history,
                            _meterFreeGames);
                        break;
                }

                _sessionInfoService.HandleTransaction(@event.Transaction);

                scope.Complete();
            }

            HandpayDisplayHelper.HandleHandpayMessageDisplay(@event.Transaction);
        }
    }
}
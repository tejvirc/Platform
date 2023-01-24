namespace Aristocrat.Monaco.Gaming.Consumers
{
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using System;

    /// <summary>
    /// The KeyedCreditOffConsumer Consumer Consumes KeyedCreditOffEvent 
    /// </summary>
    public class KeyedCreditOffConsumer : Consumes<KeyedCreditOffEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _history;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly bool _meterFreeGames;

        /// <summary>
        /// Initializes a new instance of the KeyedCreditOffConsumer class.
        /// </summary>
        /// <param name="currencyHandler"></param>
        /// <param name="gameHistory"></param>
        /// <param name="history"></param>
        /// <param name="sessionInfoService"></param>
        /// <param name="properties"></param>
        /// <param name="persistentStorage"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public KeyedCreditOffConsumer(
            ICurrencyInContainer currencyHandler,
            IGameHistory gameHistory,
            ITransactionHistory history,
            ISessionInfoService sessionInfoService,
            IPropertiesManager properties,
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
        /// <param name="event">The WatTransferCompletedEvent to handle.</param>
        public override void Consume(KeyedCreditOffEvent @event)
        {
            if (@event?.Transaction == null)
            {
                return;
            }

            var transaction = @event.Transaction;
            var amount = ((KeyedCreditsTransaction)transaction).Amount;

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                transaction.HandleOutTransaction(amount, _currencyHandler, _gameHistory, _history, _meterFreeGames);

                _sessionInfoService.HandleTransaction(@event.Transaction);

                scope.Complete();
            }

        }
    }
}

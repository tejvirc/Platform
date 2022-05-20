namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Consumer class for handling WatTransferCompletedEvent.
    /// </summary>
    public class WatTransferCommittedConsumer : Consumes<WatTransferCommittedEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _history;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IPersistentStorageManager _persistentStorage;

        private readonly bool _meterFreeGames;

        /// <summary>
        ///     Initializes a new instance of the WatTransferCompletedConsumer class.
        /// </summary>
        /// <param name="currencyHandler">The currency in container.</param>
        /// <param name="gameHistory">The game history provider.</param>
        /// <param name="history">The history provider.</param>
        /// <param name="sessionInfoService">The session information provider.</param>
        /// <param name="properties">The properties provider.</param>
        /// <param name="persistentStorage">The persistence storage manager.</param>
        public WatTransferCommittedConsumer(
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
        public override void Consume(WatTransferCommittedEvent @event)
        {
            if (@event?.Transaction == null)
            {
                return;
            }

            var transaction = @event.Transaction;
            var amount = transaction.AuthorizedCashableAmount +
                         transaction.AuthorizedNonCashAmount +
                         transaction.AuthorizedPromoAmount;


            using (var scope = _persistentStorage.ScopedTransaction())
            {
                transaction.HandleOutTransaction(amount, _currencyHandler, _gameHistory, _history, _meterFreeGames);

                _sessionInfoService.HandleTransaction(@event.Transaction);

                scope.Complete();
            }
        }
    }
}
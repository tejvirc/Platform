namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Consumer class for handling TransactionSavedEvent.
    /// </summary>
    public class TransactionSavedConsumer : Consumes<TransactionSavedEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _history;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IPersistentStorageManager _persistentStorage;

        private readonly bool _meterFreeGames;

        /// <summary>
        ///     Initializes a new instance of the TransactionSavedConsumer class.
        /// </summary>
        /// <param name="currencyHandler">The currency in container.</param>
        /// <param name="gameHistory">The game history provider.</param>
        /// <param name="history">The history provider.</param>
        /// <param name="sessionInfoService">The session information provider.</param>
        /// <param name="properties">The properties provider.</param>
        /// <param name="persistentStorage"></param>
        public TransactionSavedConsumer(
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
        /// <param name="event">The TransactionSavedEvent to handle.</param>
        public override void Consume(TransactionSavedEvent @event)
        {
            using (var scope = _persistentStorage.ScopedTransaction())
            {
                switch (@event.Transaction)
                {
                    case WatTransaction _:
                        return;
                    case WatOnTransaction _:
                        return;
                    case BillTransaction _:
                        return;
                    case VoucherOutTransaction trans:
                        trans.HandleOutTransaction(
                            trans.Amount,
                            _currencyHandler,
                            _gameHistory,
                            _history,
                            _meterFreeGames);
                        break;
                    case HandpayTransaction trans:
                        var amount = trans.TransactionAmount;
                        if (amount > 0)
                        {
                            trans.HandleOutTransaction(
                                amount,
                                _currencyHandler,
                                _gameHistory,
                                _history,
                                _meterFreeGames);
                        }
                        break;
                    case HardMeterOutTransaction trans:
                        trans.HandleOutTransaction(trans.Amount,
                            _currencyHandler,
                            _gameHistory,
                            _history,
                            _meterFreeGames);
                        break;
                }

                _sessionInfoService.HandleTransaction(@event.Transaction);

                scope.Complete();
            }
        }
    }
}
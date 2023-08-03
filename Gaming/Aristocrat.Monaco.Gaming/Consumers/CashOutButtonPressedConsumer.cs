namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Contracts;
    using log4net;

    /// <summary>
    ///     Handles the <see cref="CashOutButtonPressedEvent" />
    /// </summary>
    public class CashOutButtonPressedConsumer : Consumes<CashOutButtonPressedEvent>, IGameStartCondition
    {
        /// <summary>Create a logger for use in this class.</summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGamePlayState _gamePlayState;
        private readonly IPlayerBank _playerBank;
        private readonly IGameStartConditionProvider _gameStartConditions;
        private readonly IEventBus _eventBus;

        private readonly object _lock = new object();
        private bool _disposed;

        public bool InCashout { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CashOutButtonPressedConsumer" /> class.
        /// </summary>
        /// <param name="playerBank">An <see cref="IPlayerBank" /> instance.</param>
        /// <param name="gamePlayState">An <see cref="IGamePlayState" /> instance.</param>
        /// <param name="gameStartConditions">An <see cref="IGameStartConditionProvider" /> instance.</param>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        public CashOutButtonPressedConsumer(IPlayerBank playerBank,
                                            IGamePlayState gamePlayState,
                                            IGameStartConditionProvider gameStartConditions,
                                            IEventBus eventBus)
        {
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _gameStartConditions = gameStartConditions ?? throw new ArgumentNullException(nameof(gameStartConditions));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _gameStartConditions.AddGameStartCondition(this);
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, Handler);
        }

        private void Handler(TransferOutCompletedEvent evt)
        {
            lock (_lock)
            {
                InCashout = false;
            }
        }

        public bool CanGameStart()
        {
            lock (_lock)
            {
                return !InCashout;
            }
        }

        /// <inheritdoc />
        public override void Consume(CashOutButtonPressedEvent theEvent)
        {
            Logger.Debug("Cashout button pressed");

            lock (_lock)
            {
                if (InCashout)
                {
                    Logger.Warn("Cashout button pressed while already cashing out");
                    return;
                }

                InCashout = true;
            }

            // If we're not idle we shouldn't be handling this event
            if (!_gamePlayState.Idle && !_gamePlayState.InPresentationIdle)
            {
                Logger.Warn($"Unable to cashout. The game play state is not idle. ({_gamePlayState.CurrentState})");

                InCashout = false;
                return;
            }

            if (!_playerBank.CashOut())
            {
                Logger.Error("Player bank cashout failed");
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _gameStartConditions.RemoveGameStartCondition(this);
            }

            _disposed = true;
        }

    }
}

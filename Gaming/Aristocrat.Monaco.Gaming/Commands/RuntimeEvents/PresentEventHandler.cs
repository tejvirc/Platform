namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Payment;
    using Runtime;
    using Runtime.Client;
    using log4net;
    using PlayMode = Runtime.Client.PlayMode;

    public class PresentEventHandler : BaseEventHandler, IRuntimeEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPlayerBank _bank;
        private readonly IGameHistory _gameHistory;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameMeterManager _meters;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPlayerService _players;
        private readonly IGameCashOutRecovery _gameCashOutRecovery;
        private readonly IPropertiesManager _properties;
        private readonly IGameProvider _gameProvider;
        private readonly IRuntime _runtime;
        private readonly IEventBus _bus;
        private readonly IPaymentDeterminationProvider _largeWinDetermination;

        public PresentEventHandler(
            IPropertiesManager properties,
            ICommandHandlerFactory commandFactory,
            IRuntime runtime,
            IGamePlayState gamePlayState,
            IGameHistory gameHistory,
            IGameProvider gameProvider,
            IPersistentStorageManager persistentStorage,
            IGameMeterManager meters,
            IPlayerBank bank,
            IPlayerService players,
            IGameCashOutRecovery gameCashOutRecovery,
            IEventBus bus,
            IPaymentDeterminationProvider largeWinDetermination)
            : base(properties, commandFactory, runtime, bank, gameCashOutRecovery)
        {
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _gameCashOutRecovery = gameCashOutRecovery ?? throw new ArgumentNullException(nameof(gameCashOutRecovery));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _largeWinDetermination = largeWinDetermination ?? throw new ArgumentNullException(nameof(largeWinDetermination));
        }

        public void HandleEvent(GameRoundEvent gameRoundEvent)
        {
            switch (gameRoundEvent.Action)
            {
                case GameRoundEventAction.Completed:
                    HandleEnd(gameRoundEvent);
                    break;
                case GameRoundEventAction.Invoked:
                    HandleInvoked(gameRoundEvent);
                    break;
                case GameRoundEventAction.Pending:
                    HandlePending();
                    break;
                case GameRoundEventAction.Begin:
                    HandleBegin();
                    break;
            }
        }

        private void HandleEnd(GameRoundEvent gameRoundEvent)
        {
            var (game, denomination) = _gameProvider.GetActiveGame();
            var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);
            _bus.Publish(new GamePresentationEndedEvent(game.Id, denomination.Value, wagerCategory.Id, _gameHistory.CurrentLog));

            var result = _gameHistory.CurrentLog.UncommittedWin;

            // This signifies the end of the base game, if we haven't recorded it yet we need to update the meters, bank, etc.
            if (MeterFreeGames && !denomination.SecondaryAllowed && _gameHistory.CurrentLog.LastCommitIndex == -1)
            {
                using var scope = _persistentStorage.ScopedTransaction();
                _bank.AddWin(result);
                _meters.IncrementGamesPlayed(
                    game.Id,
                    denomination.Value,
                    wagerCategory,
                    _gameHistory.CurrentLog.UncommittedWin > 0 ? GameResult.Won : GameResult.Lost,
                    _players.HasActiveSession);

                _meters.GetMeter(game.Id, denomination.Value, GamingMeters.EgmPaidGameWonAmount)
                    .Increment(result * GamingConstants.Millicents);

                _gameHistory.CommitWin();
                CheckOutcome(result);
                scope.Complete();

                return;
            }

            if (MeterFreeGames)
            {
                if (_gameHistory.CurrentLog.AmountOut > 0)
                {
                    UpdateBalance();
                }
            }
            else
            {
                // This is the one chance we get to prevent the game from snapping the credit meter
                var winInMillicents = _gameHistory.CurrentLog.UncommittedWin * GamingConstants.Millicents;

                // Set up the default large win determination handler if needed, then use it to get the results.
                _largeWinDetermination.Handler ??= new PaymentDeterminationHandler(_properties);
                var paymentResults = _largeWinDetermination.Handler.GetPaymentResults(winInMillicents, false);
                if (paymentResults.Any(r => r.MillicentsToPayUsingLargeWinStrategy > 0))
                {
                    Logger.Debug("PendingHandpay set to true");
                    _runtime.UpdateFlag(RuntimeCondition.PendingHandpay, true);
                }
            }

            if (gameRoundEvent.PlayMode == PlayMode.Recovery)
            {
                SetAllowSubgameRound(true);
                return;
            }

            if (MeterFreeGames)
            {
                if (!CanExitRecovery())
                {
                    return;
                }

                CheckOutcome(result);
            }

            SetAllowSubgameRound(true);
        }

        private void HandleInvoked(GameRoundEvent gameRoundEvent)
        {
            if (gameRoundEvent.PlayMode != PlayMode.Normal && gameRoundEvent.PlayMode != PlayMode.Demo)
            {
                return;
            }

            if (!MeterFreeGames && _gameCashOutRecovery.HasPending)
            {
                if (!CanExitRecovery())
                {
                    return;
                }

                ClearHandpayPendingFlag();
            }
            else if (!MeterFreeGames && !_gameCashOutRecovery.HasPending &&
                     (_gamePlayState.Idle || _gamePlayState.InPresentationIdle))
            {
                ClearHandpayPendingFlag();
            }

            SetAllowSubgameRound(true);
            _gamePlayState.End(_gameHistory.CurrentLog.FinalWin);
        }

        private void HandlePending()
        {
            var (game, denomination) = _gameProvider.GetActiveGame();
            var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);
            _bus.Publish(new GameWinPresentationStartedEvent(game.Id, denomination.Value, wagerCategory.Id, _gameHistory.CurrentLog));
        }

        private void HandleBegin()
        {
            var (game, denomination) = _gameProvider.GetActiveGame();
            var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);
            _bus.Publish(new GamePresentationStartedEvent(game.Id, denomination.Value, wagerCategory.Id, _gameHistory.CurrentLog));
        }

        private void ClearHandpayPendingFlag()
        {
            // If we recovered clear any pending handpay
            Logger.Debug("PendingHandpay set to false");
            _runtime.UpdateFlag(RuntimeCondition.PendingHandpay, false);
        }
    }
}
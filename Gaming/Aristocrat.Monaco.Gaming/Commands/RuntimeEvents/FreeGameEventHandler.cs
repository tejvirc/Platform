namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using Commands;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Runtime;
    using System;
    using System.Linq;
    using Runtime.Client;
    using PlayMode = Runtime.Client.PlayMode;

    public class FreeGameEventHandler : BaseEventHandler, IRuntimeEventHandler
    {
        private readonly IEventBus _bus;
        private readonly IPlayerBank _bank;
        private readonly IGameHistory _gameHistory;
        private readonly IGameMeterManager _meters;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPlayerService _players;
        private readonly IPropertiesManager _properties;

        public FreeGameEventHandler(
            IPropertiesManager properties,
            ICommandHandlerFactory commandFactory,
            IRuntime runtime,
            IGameHistory gameHistory,
            IPersistentStorageManager persistentStorage,
            IGameMeterManager meters,
            IPlayerBank bank,
            IPlayerService players,
            IEventBus bus,
            IGameCashOutRecovery gameCashOutRecovery)
            : base(properties, commandFactory, runtime, bank, gameCashOutRecovery)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public void HandleEvent(GameRoundEvent gameRoundEvent)
        {
            switch (gameRoundEvent.Action)
            {
                case GameRoundEventAction.Begin:
                    OnBegin();
                    break;
                case GameRoundEventAction.Invoked:
                    OnInvoked(gameRoundEvent);
                    break;
                case GameRoundEventAction.Completed:
                    OnEnd(gameRoundEvent);
                    break;
            }
        }

        private void OnBegin()
        {
            _gameHistory.StartFreeGame();

            if (MeterFreeGames)
            {
                SetAllowSubgameRound(false);
            }

            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
            var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);

            _bus.Publish(new FreeGameStartedEvent(gameId, denomId, wagerCategory.Id, _gameHistory.CurrentLog));
        }

        private void OnInvoked(GameRoundEvent gameRoundEvent)
        {
            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _gameHistory.FreeGameResults((long)gameRoundEvent.Win);

                _gameHistory.AppendGameRoundEventInfo(gameRoundEvent.GameRoundInfo);

                scope.Complete();
            }
        }

        private void OnEnd(GameRoundEvent gameRoundEvent)
        {
            if (gameRoundEvent.PlayMode == PlayMode.Recovery)
            {
                _gameHistory.AppendGameRoundEventInfo(gameRoundEvent.GameRoundInfo);
                return;
            }

            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
            var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _gameHistory.AppendGameRoundEventInfo(gameRoundEvent.GameRoundInfo);

                if (MeterFreeGames)
                {
                    var freeGames = _gameHistory.CurrentLog.FreeGames.ToList();

                    var freeGame = freeGames.LastOrDefault(g => g.Result == GameResult.None);
                    if (freeGame != null)
                    {
                        // This is per GLI and what appears to be most VLT regulations.  This feels wrong, but it's normal for some jurisdictions.
                        //  ALC Regulatory Text: "The trigger game and each free game are considered separate games."

                        var result = freeGame.FinalWin;

                        _bank.AddWin(result);

                        var complete = _gameHistory.EndFreeGame();
                        if (complete != null)
                        {
                            _meters.IncrementGamesPlayed(
                                gameId,
                                denomId,
                                wagerCategory,
                                complete.Result,
                                _players.HasActiveSession);

                            _meters.GetMeter(gameId, denomId, GamingMeters.EgmPaidGameWonAmount)
                                .Increment(result * GamingConstants.Millicents);
                        }

                        CheckOutcome(result);
                    }
                    else
                    {
                        // This basically signifies a transition out of recovery
                        var winner = freeGames.LastOrDefault();
                        if (winner?.AmountOut > 0)
                        {
                            UpdateBalance();
                        }

                        if (CanExitRecovery())
                        {
                            SetAllowSubgameRound(true);
                        }
                    }
                }
                else
                {
                    _gameHistory.EndFreeGame();
                }

                scope.Complete();
            }

            _bus.Publish(new FreeGameEndedEvent(gameId, denomId, wagerCategory.Id, _gameHistory.CurrentLog));
        }
    }
}

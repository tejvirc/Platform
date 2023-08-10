namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using System;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Runtime;
    using Runtime.Client;
    using Vgt.Client12.Application.OperatorMenu;
    using PlayMode = Runtime.Client.PlayMode;

    public class PrimaryEventHandler : BaseEventHandler, IRuntimeEventHandler
    {
        private readonly IPlayerBank _bank;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IGameHistory _gameHistory;
        private readonly IGamePlayState _gamePlayState;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPropertiesManager _properties;
        private readonly IGameRecovery _recovery;
        private readonly ISystemDisableManager _systemDisableManager;

        public PrimaryEventHandler(
            IPropertiesManager properties,
            ICommandHandlerFactory commandFactory,
            IRuntime runtime,
            IGamePlayState gamePlayState,
            IGameHistory gameHistory,
            IGameRecovery recovery,
            IPersistentStorageManager persistentStorage,
            IPlayerBank bank,
            ISystemDisableManager systemDisableManager,
            IGameCashOutRecovery gameCashOutRecovery,
            IOperatorMenuLauncher operatorMenu)
            : base(properties, commandFactory, runtime, bank, gameCashOutRecovery)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
        }

        public void HandleEvent(GameRoundEvent gameRoundEvent)
        {
            Console.WriteLine($"state:{gameRoundEvent.State.ToString()}; action:{gameRoundEvent.Action.ToString()}; gameRoundInfo:{string.Join(",", gameRoundEvent.GameRoundInfo)}");
            switch (gameRoundEvent.Action)
            {
                case GameRoundEventAction.Completed:
                    HandleCompleted(gameRoundEvent);
                    break;
                case GameRoundEventAction.Begin:
                    HandleBegin(gameRoundEvent);
                    break;
                case GameRoundEventAction.Invoked:
                    HandleInvoked(gameRoundEvent);
                    break;
            }
        }

        private void HandleCompleted(GameRoundEvent gameRoundEvent)
        {
            _gameHistory.AppendGameRoundEventInfo(gameRoundEvent.GameRoundInfo);
        }

        private void HandleBegin(GameRoundEvent gameRoundEvent)
        {
            if (_recovery.IsRecovering)
            {
                if (!_systemDisableManager.IsDisabled)
                {
                    _operatorMenu.DisableKey(GamingConstants.OperatorMenuDisableKey);
                }

                // Recovery will resend us the game events (including the game round event info).  So to
                // prevent duplicate data, we need to clear the data and recovery will rebuild it.  However,
                // we still want to persist it as we go along so we have it for a dispute in case we cannot
                // recover.
                _gameHistory.ClearForRecovery();
            }

            Console.WriteLine("PrimaryEventHandler HandleBegin ..."); ;
            _gamePlayState.Start((long)gameRoundEvent.Bet, gameRoundEvent.Data, _recovery.IsRecovering);
            SetAllowSubgameRound(false);
        }

        private void HandleInvoked(GameRoundEvent gameRoundEvent)
        {
            using var scope = _persistentStorage.ScopedTransaction();
            _gameHistory.AppendGameRoundEventInfo(gameRoundEvent.GameRoundInfo);

            if (gameRoundEvent.State == GameRoundEventState.Primary)
            {
                if (gameRoundEvent.Stake > 0)
                {
                    // For secondary games we don't have a traditional start
                    _gamePlayState.StartSecondaryGame(
                        (long)gameRoundEvent.Stake,
                        (long)gameRoundEvent.Win,
                        gameRoundEvent.PlayMode == PlayMode.Recovery);
                }
                else
                {
                    if (gameRoundEvent.Win > 0)
                    {
                        _gameHistory.IncrementUncommittedWin((long)gameRoundEvent.Win);
                    }

                    if (gameRoundEvent is { Bet: > 0, PlayMode: PlayMode.Demo or PlayMode.Normal })
                    {
                        _gameHistory.AdditionalWager((long)gameRoundEvent.Bet);

                        var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
                        var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

                        _bank.Lock();

                        _commandFactory.Create<Wager>()
                            .Handle(new Wager(gameId, denomId, (long)gameRoundEvent.Bet));
                        UpdateBalance();
                    }
                }

                if (gameRoundEvent is { PlayMode: PlayMode.Demo or PlayMode.Normal, Data: { } })
                {
                    _commandFactory.Create<AddRecoveryDataPoint>()
                        .Handle(new AddRecoveryDataPoint(gameRoundEvent.Data));
                }
            }

            scope.Complete();
        }
    }
}
﻿namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Common.PerformanceCounters;
    using Contracts;
    using Contracts.Central;
    using Kernel;
    using log4net;
    using Progressives;
    using Runtime;
    using Runtime.Client;

    /// <summary>
    ///     Command handler for the <see cref="BeginGameRoundAsync" /> command.
    /// </summary>
    [CounterDescription("Game Start", PerformanceCounterType.AverageTimer32)]
    public class BeginGameRoundAsyncCommandHandler : ICommandHandler<BeginGameRoundAsync>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IGamePlayState _gamePlayState;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGameHistory _gameHistory;
        private readonly IRuntime _runtime;
        private readonly IGameRecovery _recovery;
        private readonly IGameProvider _gameProvider;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IGameStartConditionProvider _gameStartConditions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginGameRoundAsyncCommandHandler" /> class.
        /// </summary>
        public BeginGameRoundAsyncCommandHandler(
            IRuntime runtime,
            IGameRecovery recovery,
            IGamePlayState gamePlayState,
            IPropertiesManager properties,
            IGameDiagnostics diagnostics,
            IGameHistory gameHistory,
            IEventBus eventBus,
            IGameProvider gameProvider,
            IProgressiveGameProvider progressiveGameProvider,
            IGameStartConditionProvider gameStartConditions)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameDiagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _progressiveGameProvider = progressiveGameProvider ?? throw new ArgumentNullException(nameof(progressiveGameProvider));
            _gameStartConditions = gameStartConditions ?? throw new ArgumentNullException(nameof(gameStartConditions));
        }

        /// <inheritdoc />
        public void Handle(BeginGameRoundAsync command)
        {
            Task.Run(() => HandleAsync(command));
        }

        private void HandleAsync(BeginGameRoundAsync command)
        {
            if (!_recovery.IsRecovering && !_gameDiagnostics.IsActive)
            {
                if (!_gameStartConditions.CheckGameStartConditions() || !_gamePlayState.Prepare(command.Wager))
                {
                    Failed("starting conditions failed");
                    return;
                }
            }

            var (game, denomination) = _properties.GetActiveGame();
            if (game == null || denomination == null)
            {
                Failed("game is not running");
                return;
            }

            var wagerCategory = game.WagerCategories?.FirstOrDefault(x => x.Id == command.WagerCategoryId.ToString());
            SetWagerCategory(wagerCategory);

            if (command.Request is not null)
            {
                foreach (var gameInfo in command.Request.AdditionalInfo)
                {
                    if (gameInfo is not ITemplateRequest request)
                    {
                        continue;
                    }

                    if (ValidateGameInfo(gameInfo, game, request))
                    {
                        continue;
                    }

                    _gamePlayState.InitializationFailed();
                    Failed($"wager category is null: {request.TemplateId}");
                    return;
                }

                // Special case for recovery and replay
                if (_gameDiagnostics.IsActive && _gameDiagnostics.Context is IDiagnosticContext<IGameHistoryLog> context)
                {
                    Notify(context.Arguments.Outcomes);
                    return;
                }

                if (_recovery.IsRecovering && _gameHistory.CurrentLog.Outcomes.Any())
                {
                    Logger.Info($"Recovering with existing outcomes: {_gameHistory.CurrentLog.TransactionId}");
                    Notify(_gameHistory.CurrentLog.Outcomes);
                    return;
                }

                if (!_gamePlayState.EscrowWager(command.Wager, command.Data, command.Request, _recovery.IsRecovering))
                {
                    _gamePlayState.InitializationFailed();
                    Failed("EscrowWager is false");
                    return;
                }

                _runtime?.CallJackpotNotificationIfBetAmountChanged(
                    _properties,
                    Logger,
                    _progressiveGameProvider,
                    command.Wager,
                    game.Id,
                    denomination.Value.MillicentsToCents());
            }
            else
            {
                // This is required for the game round to continue.  BeginGameRoundResponse will be invoked when the outcome request completes
                Notify(Enumerable.Empty<Outcome>());
            }

            Logger.Debug("Successfully started game round");

            void Notify(IEnumerable<Outcome> outcomes)
            {
                _runtime.BeginGameRoundResponse(BeginGameRoundResult.Success, outcomes);
            }

            void Failed(string reason)
            {
                _runtime.BeginGameRoundResponse(BeginGameRoundResult.Failed, Enumerable.Empty<Outcome>());

                Logger.Warn($"Failed to start game round: {reason}");

                _eventBus.Publish(new GameRequestFailedEvent());
            }

            void SetWagerCategory(IWagerCategory category)
            {
                category ??= game.WagerCategories?.FirstOrDefault();
                _properties.SetProperty(GamingConstants.SelectedWagerCategory, category);
            }
        }

        private bool ValidateGameInfo(
            IAdditionalGamePlayInfo gameInfo,
            IGameDetail game,
            ITemplateRequest request)
        {
            if (gameInfo.GameIndex == 0)
            {
                var cdsInfo = game?.CdsGameInfos?.SingleOrDefault(
                    w =>
                        w.Id.Equals(request.TemplateId.ToString(), StringComparison.Ordinal));

                return cdsInfo is not null;
            }
            else
            {
                var currentSubGame = _gameProvider.GetEnabledSubGames(game).First(x => x.Id == gameInfo.GameId);
                var cdsInfo = currentSubGame.CdsGameInfos?.SingleOrDefault(
                    w =>
                        w.Id.Equals(request.TemplateId.ToString(), StringComparison.Ordinal));

                var subGameList = new List<ISubGameDetails> { currentSubGame };
                _gameProvider.SetActiveSubGames(game.Id, subGameList);
                _gameProvider.SetSubGameActiveDenomination(game.Id, currentSubGame, gameInfo.Denomination);

                return cdsInfo is not null;
            }
        }
    }
}
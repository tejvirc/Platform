namespace Aristocrat.Monaco.Gaming.Commands
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
    ///     Command handler for the <see cref="BeginGameRound" /> command.
    /// </summary>
    [CounterDescription("Game Start", PerformanceCounterType.AverageTimer32)]
    public class BeginGameRoundAsyncCommandHandler : ICommandHandler<BeginGameRoundAsync>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGamePlayState _gamePlayState;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGameHistory _gameHistory;
        private readonly IRuntime _runtime;
        private readonly IGameRecovery _recovery;
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
                if (!_gameStartConditions.CheckGameStartConditions() || !_gamePlayState.Prepare())
                {
                    Failed("starting conditions failed");
                    return;
                }
            }

            var (game, denomination) = _properties.GetActiveGame();

            if (command.Request is not null)
            {
                if (command.Request is ITemplateRequest request)
                {
                    var cdsInfo = game?.CdsGameInfos?.SingleOrDefault(w =>
                        w.Id.Equals(request.TemplateId.ToString()));

                    if (cdsInfo is null)
                    {
                        Failed($"wager category is null: {request.TemplateId}");
                        return;
                    }
                }

                SetWagerCategory();
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
                    Failed("EscrowWager is false");
                    return;
                }

                _runtime?.CallJackpotNotificationIfBetAmountChanged(
                    _properties,
                    Logger,
                    _progressiveGameProvider,
                    command.Wager,
                    game?.Id ?? 0,
                    denomination.Value.MillicentsToCents());
            }
            else
            {
                SetWagerCategory();

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

            void SetWagerCategory(IWagerCategory wagerCategory = null)
            {
                wagerCategory ??= game?.WagerCategories?.FirstOrDefault();

                _properties.SetProperty(GamingConstants.SelectedWagerCategory, wagerCategory);
            }
        }
    }
}
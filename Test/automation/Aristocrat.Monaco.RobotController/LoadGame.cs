namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Linq;
    using System.Threading;

    internal class LoadGame : IRobotOperations, IDisposable
    {
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly IPropertiesManager _pm;
        private readonly ILog _logger;
        private Timer _LoadGameTimer;
        private bool _disposed;
        private bool _isTimeLimitDialogVisible;

        public LoadGame(Configuration config, ILobbyStateManager lobbyStateManager, Automation automator, ILog logger, IEventBus eventBus, IPropertiesManager pm)
        {
            _config = config;
            _lobbyStateManager = lobbyStateManager;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _pm = pm;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LoadGameEvent>(this, Handler);
            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                            this,
                            evt =>
                            {
                                _isTimeLimitDialogVisible = true;
                            });

            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _isTimeLimitDialogVisible = false;
                });
        }

        private void Handler(LoadGameEvent obj)
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            if (obj.GoToNextGame)
            {
                _config.SelectNextGame();
            }
            if (!IsTimeLimitDialogInProgress())
            {
                RequestGameLoad();
            }
        }

        private bool IsTimeLimitDialogInProgress()
        {
            var timeLimitDialogVisible = _pm.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
            var timeLimitDialogPending = _pm.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            return timeLimitDialogVisible && timeLimitDialogPending;
        }

        public void Execute()
        {
            _LoadGameTimer = new Timer(
                               (sender) =>
                               {
                                   if (Validate())
                                   {
                                       _eventBus.Publish(new LoadGameEvent());
                                   }
                               },
                               null,
                               _config.Active.IntervalLoadGame,
                               Timeout.Infinite);
        }

        private bool Validate()
        {
            var isGameRuning = _lobbyStateManager.CurrentState == LobbyState.Game;
            var isGameLoading = _lobbyStateManager.CurrentState == LobbyState.GameLoading;
            var isGameDiagnostic = _lobbyStateManager.CurrentState == LobbyState.GameDiagnostics;
            var isGameLoadingForDiagnostics = _lobbyStateManager.CurrentState == LobbyState.GameLoadingForDiagnostics;
            return !isGameRuning && !isGameLoading && !isGameDiagnostic && !isGameLoadingForDiagnostics;
        }

        private void RequestGameLoad()
        {
            var games = _pm.GetValues<IGameDetail>(GamingConstants.Games).ToList();
            var gameInfo = games.FirstOrDefault(g => g.ThemeName == _config.CurrentGame && g.Enabled);
            if (gameInfo != null)
            {
                var denom = gameInfo.Denominations.First(d => d.Active == true).Value;
                _logger.Info($"Requesting game {gameInfo.ThemeName} with denom {denom} be loaded.");
                _automator.RequestGameLoad(gameInfo.Id, denom);
            }
            else
            {
                _logger.Info($"Did not find game, {_config.CurrentGame}");
                _config.SelectNextGame();
            }
        }

        public void Halt()
        {
            _LoadGameTimer?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _LoadGameTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }
    }
}

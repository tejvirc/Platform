namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;
    internal class LobbyOperations : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly RobotLogger _logger;
        private readonly StateChecker _sc;
        private readonly RobotController _robotController;
        private Timer _ForceGameExitTimer;
        private Timer _GameExitTimer;
        private bool _isTimeLimitDialogVisible;
        private bool _disposed;
        public LobbyOperations(IEventBus eventBus, RobotLogger logger, Automation automator, Configuration config, StateChecker sc, RobotController controller)
        {
            _config = config;
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = controller;
        }
        ~LobbyOperations() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _ForceGameExitTimer = new Timer(
                               (sender) =>
                               {
                                   RequestForceGameExit();
                               },
                               null,
                               _config.Active.IntervalLobby,
                               _config.Active.IntervalLobby);
            _GameExitTimer = new Timer(
                               (sender) =>
                               {
                                   RequestGameExit();
                               },
                               null,
                               _config.Active.IntervalLobby,
                               _config.Active.IntervalLobby);
        }
        private void RequestForceGameExit()
        {
            if (!IsForceGameExitValid())
            {
                _logger.Error("ForceGameExit Validation Failed", GetType().Name);
                return;
            }
            _logger.Info("ForceGameExit Requested Received!", GetType().Name);
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            _automator.ForceGameExit(Constants.GdkRuntimeHostName);
        }

        private void RequestGameExit()
        {
            if (!IsGameExitValid())
            {
                _logger.Error("RequestGameExit Validation Failed", GetType().Name);
                return;
            }
            _logger.Info("RequestGameExit Requested Received!", GetType().Name);
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            _automator.RequestGameExit();
        }
        private bool IsGameExitValid()
        {
            return _robotController.IdleDuration > 5000 && (_sc.IsGame && !_sc.IsAllowSingleGameAutoLaunch);
        }
        private bool IsForceGameExitValid()
        {
            return _sc.IsGame && _config.Active.TestRecovery;
        }
        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _GameExitTimer?.Dispose();
            _ForceGameExitTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LobbyForceGameExitRequestEvent>(this, HandleEvent);
            _eventBus.Subscribe<LobbyGameExitRequestEvent>(this, HandleEvent);
            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                this,
                evt =>
                {
                    _logger.Info("TimeLimitDialogVisibleEvent Got Triggered!", GetType().Name);
                    _isTimeLimitDialogVisible = true;
                    if (evt.IsLastPrompt)
                    {
                        _automator.EnableCashOut(true);
                    }
                });

            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _logger.Info("TimeLimitDialogHiddenEvent Got Triggered!", GetType().Name);
                    _isTimeLimitDialogVisible = false;
                });
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _logger.Info("GameInitializationCompletedEvent Got Triggered!", GetType().Name);
                    _ForceGameExitTimer?.Change(_config.Active.IntervalLoadGame, _config.Active.IntervalLoadGame);
                    _GameExitTimer?.Change(_config.Active.IntervalLobby, _config.Active.IntervalLobby);
                });
        }
        private void HandleEvent(LobbyGameExitRequestEvent obj)
        {
            RequestGameExit();
        }
        private void HandleEvent(LobbyForceGameExitRequestEvent obj)
        {
            RequestForceGameExit();
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
                _GameExitTimer?.Dispose();
                _ForceGameExitTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }
    }
}

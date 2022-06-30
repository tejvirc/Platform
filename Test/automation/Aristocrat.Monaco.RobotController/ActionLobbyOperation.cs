namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;
    internal class ActionLobbyOperation : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private readonly IPropertiesManager _pm;
        private readonly Func<long> _idleDuration;
        private Timer _ForceGameExitTimer;
        private Timer _GameExitTimer;
        private bool _isTimeLimitDialogVisible;
        private bool _disposed;
        private static ActionLobbyOperation instance = null;
        private static readonly object padlock = new object();
        public static ActionLobbyOperation Instatiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new ActionLobbyOperation(robotInfo);
                }
                return instance;
            }
        }
        private ActionLobbyOperation(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _pm = robotInfo.PropertiesManager;
            _idleDuration = robotInfo.IdleDuration;
        }
        ~ActionLobbyOperation() => Dispose(false);
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
            if (!IsForceGameExitValid()) { return; }
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            _automator.EnableExitToLobby(true);
            _automator.ForceGameExit(Constants.GdkRuntimeHostName);
        }

        private void RequestGameExit()
        {
            if (!IsGameExitValid()) { return; }
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            _automator.EnableExitToLobby(true);
            _automator.RequestGameExit();
        }
        private bool IsGameExitValid()
        {
            return _idleDuration() > 8000 && (_sc.IsGame && !_sc.IsAllowSingleGameAutoLaunch);
        }
        private bool IsForceGameExitValid()
        {
            return _sc.IsGame && _config.Active.TestRecovery;
        }
        public void Halt()
        {
            _GameExitTimer?.Dispose();
            _ForceGameExitTimer?.Dispose();
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<RequestForceGameExitEvent>(this, HandleEvent);
            _eventBus.Subscribe<RequestGameExitEvent>(this, HandleEvent);
            _eventBus.Subscribe<LobbyInitializedEvent>(this, _ => _automator.EnableExitToLobby(false));
            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                this,
                evt =>
                {
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
                    _isTimeLimitDialogVisible = false;
                });
        }
        private void HandleEvent(RequestGameExitEvent obj)
        {
            RequestGameExit();
        }
        private void HandleEvent(RequestForceGameExitEvent obj)
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

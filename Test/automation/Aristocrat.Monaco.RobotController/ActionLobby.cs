namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Application.Contracts.Operations;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ActionLobby : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private Timer _ActionLobbyTimer;
        private bool _isTimeLimitDialogVisible;
        private bool _disposed;
        private static ActionLobby instance = null;
        private static readonly object padlock = new object();
        public static ActionLobby Instatiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new ActionLobby(robotInfo);
                }
                return instance;
            }
        }
        private ActionLobby(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
        }
        ~ActionLobby() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _ActionLobbyTimer = new Timer(
                               (sender) =>
                               {
                                   if (!IsValid()) { return; }
                                   _eventBus.Publish(new ActionLobbyEvent());
                               },
                               null,
                               _config.Active.IntervalLobby,
                               _config.Active.IntervalForceGameExit);
        }
        public void Halt()
        {
            _eventBus.UnsubscribeAll(this);
            _ActionLobbyTimer?.Dispose();
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ActionLobbyEvent>(this, HandleEvent);

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
        private void HandleEvent(ActionLobbyEvent obj)
        {
            _config.SelectNextGame();
            _automator.EnableExitToLobby(true);
            _automator.ForceGameExit(Constants.GdkRuntimeHostName);
        }
        private bool IsValid()
        {
            return _sc.IsGame && _config.Active.TestRecovery && !_sc.IsAllowSingleGameAutoLaunch ;
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
                _ActionLobbyTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}

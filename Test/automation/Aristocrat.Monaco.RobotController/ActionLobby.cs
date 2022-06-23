namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Application;
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
        private readonly Configuration _config;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly Automation _automator;
        private readonly ILog _logger;
        private bool _isTimeLimitDialogVisible;
        private Timer _ActionLobbyTimer;
        private bool _disposed;
        private IEventBus _eventBus;
        private IOperatingHoursMonitor _operatingHoursMonitor;
        public ActionLobby(Configuration config, ILobbyStateManager lobbyStateManager, Automation automator, ILog logger, IEventBus eventBus)
        {
            _config = config;
            _lobbyStateManager = lobbyStateManager;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            WaitForServices();
            SubscribeToEvents();
        }
        private void WaitForServices()
        {
            Task.Run(() =>
            {
                using (var serviceWaiter = new ServiceWaiter(_eventBus))
                {
                    serviceWaiter.AddServiceToWaitFor<IOperatingHoursMonitor>();
                    if (serviceWaiter.WaitForServices())
                    {
                        _operatingHoursMonitor = ServiceManager.GetInstance().TryGetService<IOperatingHoursMonitor>();
                    }
                }
            });
        }
        private void SubscribeToEvents()
        {
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
        public string Name => typeof(ActionLobby).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ActionLobby) };

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
            }

            _disposed = true;
        }

        public void Execute()
        {
            _ActionLobbyTimer = new Timer(
                               (sender) =>
                               {
                                   if (!_lobbyStateManager.AllowSingleGameAutoLaunch)
                                   {
                                       PerformAction();
                                   }
                               },
                               null,
                               _config.Active.IntervalLobby,
                               Timeout.Infinite);
        }

        private void PerformAction()
        {
            if (_lobbyStateManager.CurrentState is LobbyState.Game)
            {
                _config.SelectNextGame();
                _automator.EnableExitToLobby(true);
                if (_config.Active.TestRecovery)
                {
                    _automator.ForceGameExit(Constants.GdkRuntimeHostName);
                }
                else
                {
                    HandlerDriveRecovery();
                }
            }

            else if (_lobbyStateManager.CurrentState is LobbyState.Chooser &&
                     !_operatingHoursMonitor.OutsideOperatingHours)
            {
                _logger.Info("Queueing game load");
                _config.SelectNextGame();
                _automator.EnableExitToLobby(false);
                //TODO:RequestGameLoad using statemangager
            }
        }

        //TODO: retest this
        private void HandlerDriveRecovery()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            var isInRecoveryState = _lobbyStateManager.CurrentState is LobbyState.Recovery || _lobbyStateManager.CurrentState is LobbyState.RecoveryFromStartup;
            var isInGameState = _lobbyStateManager.CurrentState is LobbyState.Game || _lobbyStateManager.CurrentState is LobbyState.GameLoading;
            if (isInRecoveryState || _lobbyStateManager.CurrentState is LobbyState.Game)
            {
                //TODO Make ActionTouchEvent
                // Some recovery scenarios require touch
                //DriveToIdle();
            }
            // If we cause recovery, let it play out
            else if (isInGameState)
            {
                HandlerRecoveryComplete();
            }
        }

        private void HandlerRecoveryComplete()
        {
            //_waitDuration += _config.Active.IntervalResolution;

            //_automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);

            //if (_waitDuration >= 4000 && PlatformState == RobotPlatformState.GamePlaying)
            //{
            //    _waitDuration = 0;

            //    // If a game is being played after 4 seconds, go back to driving recovery. A free game feature may have started.
            //    ControllerState = (RobotControllerState.DriveRecovery);
            //}
            //else if (_waitDuration >= 8000 && PlatformState != RobotPlatformState.GamePlaying)
            //{
            //    _waitDuration = 0;
            //    if (_config.ActiveType == ModeType.Super || _config.ActiveType == ModeType.Uber)
            //    {
            //        if (PlatformState != RobotPlatformState.Lobby && _gameLoaded)
            //        {
            //            ControllerState = (RobotControllerState.RequestGameExit);
            //        }
            //        else
            //        {
            //            ControllerState = (RobotControllerState.RequestGameLoad);
            //        }
            //    }
            //    else
            //    {
            //        LogInfo("Recovery complete but no game playing. Disabling.");
            //        Enabled = false;
            //    }
            //}
        }
        public void Halt()
        {
            _ActionLobbyTimer?.Dispose();
        }

        public void Initialize()
        {
        }
    }
}

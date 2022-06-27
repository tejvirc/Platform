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
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly IPropertiesManager _pm;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        //todo: Handle this
        //private IOperatingHoursMonitor _operatingHoursMonitor;
        private Timer _LoadGameTimer;
        private bool _disposed;
        private bool _isTimeLimitDialogVisible;
        private int sanityCounter;
        private bool _enabled;
        private static LoadGame instance = null;
        private static readonly object padlock = new object();
        public static LoadGame Instatiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new LoadGame(robotInfo);
                }
                return instance;
            }
        }
        private LoadGame(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _pm = robotInfo.PropertiesManager;
        }
        ~LoadGame() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _LoadGameTimer = new Timer(
                               (sender) =>
                               {
                                   if (!_enabled || !IsValid()) { return; }
                                   _eventBus.Publish(new LoadGameEvent(true));
                               },
                               null,
                               1000,
                               _config.Active.IntervalLoadGame);
            _enabled = true;
        }
        public void Halt()
        {
            _enabled = false;
            _LoadGameTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
            sanityCounter = 0;
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LoadGameEvent>(this, HandleEvent);
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

            _eventBus.Subscribe<GameRequestFailedEvent>(
                           this,
                           _ =>
                           {
                               if ( !_sc.IsAllowSingleGameAutoLaunch)
                               {
                                   sanityCounter++;
                                   _eventBus.Publish(new LoadGameEvent(true));
                               }
                           });
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                           this,
                           _ =>
                           {
                               sanityCounter = 0;
                           });
        }
        private void HandleEvent(LoadGameEvent evt)
        {
            if (!IsValid())
            {
                //Todo: Log Something
                return;
            }
            DismissTimeLimitDialog();
            SelectGame(evt);
            if (!IsTimeLimitDialogInProgress() && CheckSanity())
            {
                RequestGameLoad();
            }
        }
        private void DismissTimeLimitDialog()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
        }
        private void SelectGame(LoadGameEvent evt)
        {
            if (evt.GoToNextGame)
            {
                _config.SelectNextGame();
            }
        }
        private bool IsTimeLimitDialogInProgress()
        {
            var timeLimitDialogVisible = _pm.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
            var timeLimitDialogPending = _pm.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            return timeLimitDialogVisible && timeLimitDialogPending;
        }
        private bool IsValid()
        {
            return _sc.IsChooser;
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
                sanityCounter++;
                _logger.Info($"Did not find game, {_config.CurrentGame}");
                _eventBus.Publish(new LoadGameEvent(true));
            }
        }
        private bool CheckSanity()
        {
            if (sanityCounter < 10) { return true; }
            Halt();
            return false;
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

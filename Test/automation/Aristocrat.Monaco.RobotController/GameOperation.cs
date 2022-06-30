namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Hhr.Events;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class GameOperation : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly IPropertiesManager _pm;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private Action _idleDurationReset;
        private Timer _LoadGameTimer;
        private bool _disposed;
        private bool _isTimeLimitDialogVisible;
        private int sanityCounter;
        private static GameOperation instance = null;
        private static readonly object padlock = new object();
        public static GameOperation Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new GameOperation(robotInfo);
                }
                return instance;
            }
        }
        private GameOperation(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _pm = robotInfo.PropertiesManager;
            _idleDurationReset = robotInfo.IdleDurationReset;
        }
        ~GameOperation() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _LoadGameTimer = new Timer(
                               (sender) =>
                               {
                                   HandleGameRequest();
                               },
                               null,
                               _config.Active.IntervalLoadGame,
                               _config.Active.IntervalLoadGame);
        }
        private void HandleGameRequest(LoadGameEvent evt = null)
        {
            if (!IsValid())
            {
                //Todo: Log Something
                return;
            }
            evt = evt ?? new LoadGameEvent(true);
            DismissTimeLimitDialog();
            SelectGame(evt);
            if (!IsTimeLimitDialogInProgress() && CheckSanity())
            {
                RequestGameLoad();
            }
        }
        public void Halt()
        {
            _LoadGameTimer?.Dispose();
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
                    if (!_sc.IsAllowSingleGameAutoLaunch)
                    {
                        sanityCounter++;
                        HandleGameRequest();
                    }
                });
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {//2
                    _eventBus.Publish(new BalanceCheckEvent());
                    sanityCounter = 0;
                });
            _eventBus.Subscribe<GameDisabledEvent>
                                        (this, _ =>
                                        {
                                        });
            _eventBus.Subscribe<GameEnabledEvent>
                                                    (this, _ =>
                                                    {
                                                    });

            _eventBus.Subscribe<GamePlayRequestFailedEvent>(this, _ =>
                {
                    _logger.Info("Keying off GamePlayRequestFailed");
                    ToggleJackpotKey(1000);
                });

            _eventBus.Subscribe<UnexpectedOrNoResponseEvent>(this, _ =>
                {
                    _logger.Info("Keying off UnexpectedOrNoResponseEvent");
                    ToggleJackpotKey(10000);
                });

            _eventBus.Subscribe<GameExitedNormalEvent>(
                           this,
                           _ =>
                           {
                               
                           });
            _eventBus.Subscribe<GameIdleEvent>(
                            this,
                            _ =>
                            {//5
                                _eventBus.Publish(new BalanceCheckEvent());
                                if ((bool)_pm.GetProperty("Automation.HandleExitToLobby", false))
                                {
                                    //_automator.RequestGameExit();
                                }
                            });
            _eventBus.Subscribe<GameSelectedEvent>(
                            this,
                            evt =>
                            {
                                //add log 1
                            });
            _eventBus.Subscribe<RecoveryStartedEvent>(
                            this,
                            _ =>
                            {
                                //log
                            });
            _eventBus.Subscribe<GameProcessExitedEvent>(
                                        this,
                                        evt =>
                                        {
                                            //log
                                            if (evt.Unexpected) {
                                                _automator.EnableExitToLobby(true);
                                            } else {
                                                _automator.EnableExitToLobby(false);
                                            }
                                            Task.Run(() =>
                                            {
                                                Thread.Sleep(2000);
                                                HandleGameRequest();
                                            });
                                        });
            _eventBus.Subscribe<GameFatalErrorEvent>(
                                                    this,
                                                    _ =>
                                                    {
                                                        //log
                                                    });
            _eventBus.Subscribe<GamePlayStateChangedEvent>(
                                                                this,
                                                                _ =>
                                                                {
                                                                    //log
                                                                    _idleDurationReset();
                                                                });
            _eventBus.Subscribe<GameRequestFailedEvent>(
                                                                            this,
                                                                            _ =>
                                                                            {
                                                                                //log
                                                                            });
        }
        private void ToggleJackpotKey(int waitDuration)
        {
            Task.Delay(waitDuration).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
        }
        private void HandleEvent(LoadGameEvent evt)
        {
            HandleGameRequest(evt);
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

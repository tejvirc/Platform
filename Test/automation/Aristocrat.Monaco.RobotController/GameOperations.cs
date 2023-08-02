namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Hhr.Events;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Security.Cryptography;

    internal class GameOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly IPropertiesManager _propertyManager;
        private readonly RobotLogger _logger;
        private readonly StateChecker _stateChecker;
        private readonly RobotController _robotController;
        private readonly IGameService _gameService;
        private bool _goToNextGame;
        private Timer _loadGameTimer;
        private Timer _RgTimer;
        private Timer _forceGameExitTimer;
        private bool _disposed;
        private bool _isTimeLimitDialogVisible;
        private int _sanityCounter;
        private bool _exitWhenIdle;
        private bool _forceGameExitIsInProgress;
        private bool _requestGameIsInProgress;
        private bool _gameIsRunning;
        private const int RandomNumberMax = 2; 

        private readonly int CashoutDialogDismiss = (int)TimeSpan.FromSeconds(3).TotalMilliseconds;

        public GameOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, IPropertiesManager pm, RobotController robotController, IGameService gameService)
        {
            _stateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _propertyManager = pm;
            _robotController = robotController;
            _gameService = gameService;
        }

        ~GameOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Execute()
        {
            _logger.Info("GameOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();

            if (IsRegularRobots())
            {
                return;
            }

            _loadGameTimer = new Timer(
                               (sender) =>
                               {
                                   RequestGame();
                               },
                               null,
                               _robotController.Config.Active.IntervalLoadGame,
                               _robotController.Config.Active.IntervalLoadGame);
            _RgTimer = new Timer(
                               (sender) =>
                               {
                                   RequestRg();
                               },
                               null,
                               _robotController.Config.Active.IntervalRgSet,
                               _robotController.Config.Active.IntervalRgSet);
            _forceGameExitTimer = new Timer(
                               (sender) =>
                               {
                                   RequestForceExitToLobby();
                               },
                               null,
                               _robotController.Config.Active.IntervalLobby,
                               _robotController.Config.Active.IntervalLobby);
            LoadGameWithDelay(Constants.loadGameDelayDuration);
        }

        public void Reset()
        {
            _disposed = false;
            _sanityCounter = 0;
            _exitWhenIdle = false;
            _requestGameIsInProgress = false;
            _gameIsRunning = _gameService.Running;
            _goToNextGame = false;
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _loadGameTimer?.Dispose();
            _RgTimer?.Dispose();
            _forceGameExitTimer?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_loadGameTimer is not null)
                {
                    _loadGameTimer.Dispose();
                }
                _loadGameTimer = null;
                if (_RgTimer is not null)
                {
                    _RgTimer.Dispose();
                }
                _RgTimer = null;
                if (_forceGameExitTimer is not null)
                {
                    _forceGameExitTimer.Dispose();
                }
                _forceGameExitTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestForceExitToLobby(bool skipTestRecovery = false)
        {
            if (!IsRequestForceExitToLobbyValid(skipTestRecovery))
            {
                return;
            }
            _logger.Info("ForceGameExit Requested Received!", GetType().Name);
            _forceGameExitIsInProgress = true;
            _exitWhenIdle = false;
            _automator.ForceGameExit(Constants.GdkRuntimeHostName);
            _robotController.BlockOtherOperations(RobotStateAndOperations.GameExiting);
        }

        private bool IsRequestForceExitToLobbyValid(bool skipTestRecovery)
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation( new List<RobotStateAndOperations>());
            var isGeneralRule = (_gameIsRunning && !_stateChecker.IsGameLoading && !_forceGameExitIsInProgress && !_exitWhenIdle &&(_robotController.Config.Active.TestRecovery || skipTestRecovery));
            return !isBlocked && isGeneralRule;
        }

        private void RequestRg()
        {
            if (!_gameIsRunning)
            {
                return;
            }
            _logger.Info($"Performing Responsible Gaming Request Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
            _automator.SetResponsibleGamingTimeElapsed(_robotController.Config.GetTimeElapsedOverride());
            if (_robotController.Config.GetSessionCountOverride() != 0)
            {
                _automator.SetRgSessionCountOverride(_robotController.Config.GetSessionCountOverride());
            }
        }

        private void RequestGame()
        {
            if (IsRegularRobots())
            {
                return;
            }
            else if (!IsRequestGameValid())
            {
                _logger.Info($"RequestGame was not valid!", GetType().Name);
                return;
            }
            if (_stateChecker.IsGame && _gameIsRunning)
            {
                _logger.Info($"Exit To Lobby When Idle Requested Received! Sanity Counter = {_sanityCounter}, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                _exitWhenIdle = !IsRegularRobots();
            }
            else if (_gameIsRunning)
            {
                _logger.Info($"lobby is saying that it is in the chooser state but the game is still running, this reset the lobbystatemanager state ,Counter = {_sanityCounter}, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                RequestForceExitToLobby(true);
            }
            else
            {
                _logger.Info($"LoadGame Requested Received! Sanity Counter = {_sanityCounter}", GetType().Name);
                _sanityCounter++;
                LoadGame();
            }
        }

        private void LoadGame()
        {
            if (!IsTimeLimitDialogInProgress() && CheckSanity())
            {
                DismissTimeLimitDialog();
                ExecuteGameLoad();
            }
        }

        private void LoadGameWithDelay(int milliseconds)
        {
            _logger.Info($"LoadGameWithDelay Request is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(
                _ =>
                {
                     _requestGameIsInProgress = false;
                    RequestGame();
                });
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                 this,
                 evt =>
                 {
                     _logger.Info($"TimeLimitDialogVisibleEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _isTimeLimitDialogVisible = true;
                     DismissTimeLimitDialog();
                     if (evt.IsLastPrompt)
                     {
                         _exitWhenIdle = !IsRegularRobots();
                     }
                 });
            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _logger.Info($"TimeLimitDialogHiddenEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _isTimeLimitDialogVisible = false;
                });
            _eventBus.Subscribe<GameRequestFailedEvent>(
                this,
                _ =>
                {
                    _logger.Error($"GameRequestFailedEvent Got Triggered!  Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _requestGameIsInProgress = false;
                    if (!_stateChecker.IsAllowSingleGameAutoLaunch)
                    {
                        RequestGame();
                    }
                });
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _logger.Info($"GameInitializationCompletedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _gameIsRunning = true;
                    _sanityCounter = 0;
                     _requestGameIsInProgress = false;
                    BalanceCheckWithDelay(Constants.BalanceCheckDelayDuration);
                });

            _eventBus.Subscribe<GamePlayRequestFailedEvent>(
                this,
                _ =>
                {
                    _logger.Info($"Keying off GamePlayRequestFailed! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    ToggleJackpotKey(Constants.ToggleJackpotKeyDuration);
                });

            _eventBus.Subscribe<UnexpectedOrNoResponseEvent>(
                this,
                _ =>
                {
                    _logger.Info($"Keying off UnexpectedOrNoResponseEvent Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    ToggleJackpotKey(Constants.ToggleJackpotKeyLongerDuration);
                });
            _eventBus.Subscribe<GameIdleEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info($"GameIdleEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     BalanceCheckWithDelay(Constants.BalanceCheckDelayDuration);
                     HandleExitToLobbyRequest();
                 });
            _eventBus.Subscribe<GameProcessExitedEvent>(
                 this,
                 evt =>
                 {
                     _gameIsRunning = false;
                     _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameExiting);
                     if (evt.Unexpected)
                     {
                         if (!_forceGameExitIsInProgress)
                         {
                             _logger.Error($"GameProcessExitedEvent-Unexpected Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                             _robotController.Enabled = false;
                             return;
                         }
                         _logger.Info($"GameProcessExitedEvent-Unexpected-ForceGameExit Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                         _forceGameExitIsInProgress = false;
                         _goToNextGame = false;
                         _exitWhenIdle = !IsRegularRobots();
                     }
                     else
                     {
                         _logger.Info($"GameProcessExitedEvent-Normal Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                         _goToNextGame = !IsRegularRobots();
                     }
                     LoadGameWithDelay(Constants.loadGameDelayDuration);
                 });
            _eventBus.Subscribe<GameFatalErrorEvent>(
                 this,
                 _ =>
                 {
                     _logger.Error($"GameFatalErrorEvent Got Triggered! There is an issue with [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _robotController.Enabled = false;
                 });
            _eventBus.Subscribe<GamePlayStateChangedEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info($"GamePlayStateChangedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _robotController.IdleDuration = 0;
                     _sanityCounter = 0;
                 });
            _eventBus.Subscribe<HandpayStartedEvent>(this, evt =>
            {
                if (evt.Handpay == HandpayType.GameWin ||
                     evt.Handpay == HandpayType.CancelCredit)
                {
                    _logger.Info($"Keying off large win Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    ToggleJackpotKey(Constants.ToggleJackpotKeyDuration);
                }
                else
                {
                    _logger.Info($"Skip toggling jackpot key since evt.Handpay = [{evt.Handpay}] is not valid!", GetType().Name);
                }
            });
            _eventBus.Subscribe<SystemEnabledEvent>(
                this,
                _ =>
                {
                    _logger.Info($"SystemEnabledEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    LoadGameWithDelay(Constants.loadGameDelayDuration);
                });

            CashoutBannerSupport();

            InitGameProcessHungEvent();
        }

        private void InitGameProcessHungEvent()
        {
            // If the runtime process hangs, and the setting to not kill it is active, then stop the robot. 
            // This will allow someone to attach a debugger to investigate.
            var doNotKillRuntime = _propertyManager.GetValue("doNotKillRuntime", Common.Constants.False).ToUpperInvariant();
            if (doNotKillRuntime == Common.Constants.True)
            {
                _eventBus.Subscribe<GameProcessHungEvent>(this, _ =>
                {
                    _robotController.Enabled = false;
                });
            };
        }

        private void CashoutBannerSupport()
        {
            _eventBus.Subscribe<CashoutAmountAuthorizationRequestedEvent>(this,
                evt =>
                {
                    _robotController.BlockOtherOperations(RobotStateAndOperations.CashoutBannerSupport);
                    Task.Delay(CashoutDialogDismiss).ContinueWith(task =>
                    {
                        var cashOut = GetRandomBoolean();
                            
                        _eventBus.Publish(new CashoutAmountAuthorizationReceivedEvent(cashOut));
                        _robotController.UnBlockOtherOperations(RobotStateAndOperations.CashoutBannerSupport);
                    });
                });
        }

        private void SelectNextGame(bool goToNextGame)
        {
            if (!goToNextGame)
            {
                return;
            }
            _logger.Info("SelectNextGame Request Is Received!", GetType().Name);
            _robotController.Config.SelectNextGame();
        }

        private void HandleExitToLobbyRequest()
        {
            if (_exitWhenIdle)
            {
                _robotController.BlockOtherOperations(RobotStateAndOperations.GameExiting);
                if (!IsExitToLobbyWhenIdleValid())
                {
                    _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameExiting);
                    return;
                }
                _logger.Info($"ExitToLobby Request Is Received! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                _automator.RequestGameExit();
                _exitWhenIdle = false;
                //GameProcessExitedEvent gets trigered
            }
        }

        private bool IsExitToLobbyWhenIdleValid()
        {
            return _gameIsRunning && (_stateChecker.IsIdle || _stateChecker.IsPresentationIdle) && _exitWhenIdle && !_forceGameExitIsInProgress;
        }

        private void BalanceCheckWithDelay(int milliseconds)
        {
            _logger.Info("BalanceCheckWithDelay Request Is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(_ => BalanceCheck());
        }

        private void BalanceCheck()
        {
            _eventBus.Publish(new BalanceCheckEvent());
        }

        private void ToggleJackpotKey(int waitDuration)
        {
            _logger.Info($"ToggleJackpotKey Request Is Received! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
            Task.Delay(waitDuration).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
        }

        private void DismissTimeLimitDialog()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
        }

        private bool IsTimeLimitDialogInProgress()
        {
            var timeLimitDialogVisible = _propertyManager.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
            var timeLimitDialogPending = _propertyManager.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            return timeLimitDialogVisible && timeLimitDialogPending;
        }

        private bool IsRequestGameValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            var isGeneralRule = _stateChecker.IsChooser || (_gameIsRunning && !_stateChecker.IsGameLoading);
            return !isBlocked && isGeneralRule && !_requestGameIsInProgress;
        }

        private void ExecuteGameLoad()
        {
            _requestGameIsInProgress = true;
            SelectNextGame(_goToNextGame);
            _goToNextGame = false;
            var games = _propertyManager.GetValues<IGameDetail>(GamingConstants.Games).ToList();
            var gameInfo = games.FirstOrDefault(g => g.ThemeName == _robotController.Config.CurrentGame && g.Enabled);
            if (gameInfo != null)
            {
                var denom = gameInfo.Denominations.Where(d => d.Active == true).RandomElement().Value;
                _logger.Info($"Requesting game {gameInfo.ThemeName} with denom {denom} be loaded.", GetType().Name);
                if (gameInfo.GameType is not (Gaming.Contracts.Models.GameType)GameType.Reel)
                {
                    _automator.ResetSpeed();
                }
                else
                {
                    _automator.SetSpeed(_robotController.Config.Speed);
                }
                _automator.RequestGameLoad(gameInfo.Id, denom);
            }
            else
            {
                _logger.Error($"Did not find game, {_robotController.Config.CurrentGame}", GetType().Name);
                _robotController.Enabled = false;
            }
        }

        private bool CheckSanity()
        {
            if (_sanityCounter < 5)
            {
                return true;
            }
            _logger.Error($"Game Operation Failed, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
            _robotController.Enabled = false;
            return false;
        }

        private bool IsRegularRobots()
        {
            return _robotController.InProgressRequests.Contains(RobotStateAndOperations.RegularMode);
        }

        private bool GetRandomBoolean()
        {
            var rndGenerator = RandomNumberGenerator.Create();

            var bytes = new byte[2];

            rndGenerator.GetBytes(bytes);

            ushort ranNumber = BitConverter.ToUInt16(bytes, 0);

            return ranNumber % 2 != 0;
        }
        
    }
}

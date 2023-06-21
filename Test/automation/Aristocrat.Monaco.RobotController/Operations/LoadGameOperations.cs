namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class LoadGameOperations : IRobotOperations
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
        private bool _disposed;
        private bool _isTimeLimitDialogVisible;
        private int _sanityCounter;
        private bool _exitWhenIdle;
        private bool _forceGameExitIsInProgress;
        private bool _requestGameIsInProgress;
        private bool _gameIsRunning;

        public LoadGameOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, IPropertiesManager pm, RobotController robotController, IGameService gameService)
        {
            _stateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _propertyManager = pm;
            _robotController = robotController;
            _gameService = gameService;
        }

        ~LoadGameOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Execute()
        {
            _logger.Info("LoadGameOperation Has Been Initiated!", GetType().Name);
            SubscribeToEvents();

            _loadGameTimer = new Timer(
                               (sender) =>
                               {
                                   RequestGame();
                               },
                               null,
                               _robotController.Config.Active.IntervalLoadGame,
                               _robotController.Config.Active.IntervalLoadGame);
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
            Dispose();
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
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestGame()
        {
            if (!IsRequestGameValid())
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

        private void SelectNextGame(bool goToNextGame)
        {
            if (!goToNextGame)
            {
                return;
            }
            _logger.Info("SelectNextGame Request Is Received!", GetType().Name);
            _robotController.Config.SelectNextGame();
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
    }
}

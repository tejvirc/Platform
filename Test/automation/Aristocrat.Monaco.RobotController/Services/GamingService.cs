namespace Aristocrat.Monaco.RobotController.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;

    internal class GamingService
    {
        private readonly IEventBus _eventBus;
        private readonly RobotController _robotController;
        private readonly StateChecker _stateChecker;
        private readonly RobotService _robotService;
        private readonly RobotLogger _logger;
        private readonly IPropertiesManager _propertiesManager;
        private readonly Automation _automator;

        private uint _sanityCounter;
        private bool _gotoNextGame;
        private bool _isGameRunning;
        private bool _isLoadGameInProgress;
        private bool _toKillGamingProcessIfIdle;
        private bool _isGameExitInProgress;
        private bool _isTimeLimitDialogVisible;

        public GamingService(IEventBus eventBus, RobotController robotController, RobotService robotService, StateChecker stateChecker,
            IPropertiesManager pm, RobotLogger logger, Automation automator)
        {
            _eventBus = eventBus;
            _robotController = robotController;

            _stateChecker = stateChecker;
            _robotService = robotService;
            _logger = logger;
            _propertiesManager = pm;
            _automator = automator;
        }

        public bool SelectNextGame
        {
            get => _gotoNextGame;
            set => _gotoNextGame = value;
        }

        public bool IsTimeLimitDialogVisible
        {
            get => _isTimeLimitDialogVisible;
            set => _isTimeLimitDialogVisible = value;
        }

        public bool IsGameRunning
        {
            get => _isGameRunning;
            set => _isGameRunning = value;
        }

        public bool CanRequestGameLoad()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            var isGeneralRule = _stateChecker.IsChooser || (IsGameRunning && !_stateChecker.IsGameLoading);
            return !isBlocked && isGeneralRule && !_isLoadGameInProgress;
        }

        public void RequestGameLoad(bool selectNextGame = false)
        {
            if (!CanRequestGameLoad())
            {
                _logger.Info($"RequestGame was not valid!", GetType().Name);
                return;
            }
            if (_stateChecker.IsLobbyStateGame && IsGameRunning)
            {
                _logger.Info($"Exit To Lobby When Idle Requested Received! Sanity Counter = {_sanityCounter}, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                _toKillGamingProcessIfIdle = !_robotService.IsRegularRobots();
            }
            else if (IsGameRunning)
            {
                _logger.Info($"lobby is saying that it is in the chooser state but the game is still running, this reset the lobbystatemanager state ,Counter = {_sanityCounter}, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                KillGame(true);
            }
            else
            {
                _logger.Info($"LoadGame Requested Received! Sanity Counter = {_sanityCounter}", GetType().Name);
                _sanityCounter++;
                LoadGame();
            }
        }

        public void LoadGameWithDelay(int milliseconds, bool selectNextGame = false)
        {
            _logger.Info($"LoadGameWithDelay Request is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(
                _ =>
                {
                    _isLoadGameInProgress = false;
                    RequestGameLoad(selectNextGame);
                });
        }


        public void LoadGame()
        {
            if (!IsTimeLimitDialogInProgress() && CheckSanity())
            {
                DismissTimeLimitDialog();
                ExecuteGameLoad();
            }
        }

        private bool CanKillGame(bool skipTestRecovery)
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());

            var canExit = IsGameRunning
                && !_stateChecker.IsGameLoading
                && !_isGameExitInProgress
                && _robotService.IsRegularRobots()  // Regular mode only ?
                && (_robotController.Config.Active.TestRecovery || skipTestRecovery);

            return !isBlocked && canExit;
        }

        public void KillGame(bool skipTestRecovery = false)
        {
            if (!CanKillGame(skipTestRecovery))
            {
                return;
            }

            _logger.Info("ForceGameExit Requested Received!", GetType().Name);

            _isGameExitInProgress = true;

            _automator.KillGameProcess(Constants.GdkRuntimeHostName);

            _robotController.BlockOtherOperations(RobotStateAndOperations.GameExiting);
        }

        public void ExecuteGameLoad()
        {
            _isLoadGameInProgress = true;
            SetCurrentGame(SelectNextGame);
            
            var games = _propertiesManager.GetValues<IGameDetail>(GamingConstants.Games).ToList();
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

        public void SetCurrentGame(bool selectNextGame)
        {
            if (!selectNextGame)
            {
                return;
            }
            _logger.Info("SelectNextGame Request Is Received!", GetType().Name);
            _robotController.Config.SetCurrentGame();
        }

        public void DismissTimeLimitDialog()
        {
            _automator.DismissTimeLimitDialog(IsTimeLimitDialogVisible);
        }

        private bool IsTimeLimitDialogInProgress()
        {
            var timeLimitDialogVisible = _propertiesManager.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
            var timeLimitDialogPending = _propertiesManager.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            return timeLimitDialogVisible && timeLimitDialogPending;
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

        public void BalanceCheckWithDelay(int milliseconds)
        {
            _logger.Info("BalanceCheckWithDelay Request Is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(_ => BalanceCheck());
        }

        public void BalanceCheck()
        {
            _eventBus.Publish(new BalanceCheckEvent());
        }

    }
}

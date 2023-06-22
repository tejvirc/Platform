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
        private readonly LobbyStateChecker _lobbyStateChecker;
        private readonly RobotService _robotService;
        private readonly RobotLogger _logger;
        private readonly IPropertiesManager _propertiesManager;
        private readonly Automation _automator;
        private readonly StatusManager _statusManager;

        //private uint _statusManager.SanityCounter;
        //private bool _goingNextGame;
        //private bool _isGameRunning;
        //private bool _isLoadGameInProgress;
        //private bool _exitToLobbyWhenGameIdle;
        //private bool _isGameExitInProgress;

        public GamingService(IEventBus eventBus, RobotController robotController, RobotService robotService, LobbyStateChecker stateChecker,
            IPropertiesManager pm, RobotLogger logger, Automation automator, StatusManager robotStatusManager)
        {
            _eventBus = eventBus;
            _robotController = robotController;

            _lobbyStateChecker = stateChecker;
            _robotService = robotService;
            _logger = logger;
            _propertiesManager = pm;
            _automator = automator;
            _statusManager = robotStatusManager;
        }

        public bool CanRequestGameLoad()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            var isGeneralRule = _lobbyStateChecker.IsChooser || (_statusManager.IsGameRunning && !_lobbyStateChecker.IsGameLoading);
            return !isBlocked && isGeneralRule && !_statusManager.IsLoadGameInProgress;
        }

        public void RequestGameLoad()
        {
            if (!CanRequestGameLoad())
            {
                _logger.Info($"RequestGame was not valid!", GetType().Name);
                return;
            }
            if (_lobbyStateChecker.IsLobbyStateGame && _statusManager.IsGameRunning)
            {
                _logger.Info($"Exit To Lobby When Idle Requested Received! Sanity Counter = {_statusManager.SanityCounter}, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                _statusManager.ExitToLobbyWhenGameIdle = !_robotService.IsRegularRobots();
            }
            else if (_statusManager.IsGameRunning)
            {
                _logger.Info($"lobby is saying that it is in the chooser state but the game is still running, this reset the lobbystatemanager state ,Counter = {_statusManager.SanityCounter}, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);

                if (!CanKillGame(true))
                {
                    return;
                }

                KillGame(true);
            }
            else
            {
                _logger.Info($"LoadGame Requested Received! Sanity Counter = {_statusManager.SanityCounter}", GetType().Name);
                _statusManager.SanityCounter++;
                LoadGame();
            }
        }

        public void LoadGameWithDelay(int milliseconds, bool selectNextGame = false)
        {
            _logger.Info($"LoadGameWithDelay Request is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(
                _ =>
                {
                    // this seems can be omitted
                    _statusManager.IsLoadGameInProgress = false;
                    RequestGameLoad();
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

        public bool CanKillGame(bool skipTestRecovery = false)
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());

            var canExit = _statusManager.IsGameRunning
                && !_lobbyStateChecker.IsGameLoading
                && !_statusManager.IsGameExitInProgress
                && !_statusManager.ExitToLobbyWhenGameIdle  // shows this is on Regular mode
                && (_robotController.Config.ActiveGameMode.TestRecovery || skipTestRecovery);

            return !isBlocked && canExit;
        }

        public void KillGame(bool skipTestRecovery = false)
        {
            _logger.Info("ForceGameExit Requested Received!", GetType().Name);

            _statusManager.IsGameExitInProgress = true;

            _statusManager.ExitToLobbyWhenGameIdle = false;

            _automator.KillGameProcess(Constants.GdkRuntimeHostName);

            _robotController.BlockOtherOperations(RobotStateAndOperations.GameExiting);
        }

        public void ExecuteGameLoad()
        {
            _statusManager.IsLoadGameInProgress = true;
            SetCurrentGame(_statusManager.GoingNextGame);

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
            _automator.DismissTimeLimitDialog(_statusManager.IsTimeLimitDialogVisible);
        }

        private bool IsTimeLimitDialogInProgress()
        {
            var timeLimitDialogVisible = _propertiesManager.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
            var timeLimitDialogPending = _propertiesManager.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            return timeLimitDialogVisible && timeLimitDialogPending;
        }

        private bool CheckSanity()
        {
            if (_statusManager.SanityCounter < 5)
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

namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class LoadGameOperations : IRobotOperations
    {
        protected readonly Automation _automator;
        protected readonly IEventBus _eventBus;
        protected readonly IGameService _gameService;
        protected readonly IPropertiesManager _propertyManager;
        protected readonly RobotController _robotController;
        protected readonly RobotLogger _logger;
        protected readonly StateChecker _stateChecker;
        public bool _disposed;
        private GameOperations _gameOperation;
        private Timer _loadGameTimer;

        public LoadGameOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, IPropertiesManager pm, RobotController robotController, IGameService gameService, GameOperations gameOperation)
        {
            _gameOperation = gameOperation;
            _logger = logger;
            _eventBus = eventBus;
            _automator = automator;
            _stateChecker = sc;
            _robotController = robotController;
            _gameService = gameService;
            _propertyManager = pm;
        }

        ~LoadGameOperations() => Dispose(false);

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
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            _disposed = false;
            _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameOperation_LoadGame);
        }

        public void Execute()
        {

            if (_robotController.Config.Active.IntervalLoadGame == 0)
            {
                _logger.Info("LoadGameOperations is Disabled!", GetType().Name);
                return;
            }

            _logger.Info("LoadGameOperations Has Been Initiated!", GetType().Name);

            SubscribeToEvents();

            _loadGameTimer = new Timer(
                               (sender) =>
                               {
                                   RequestGame();
                               },
                               null,
                               _robotController.Config.Active.IntervalLoadGame,
                               _robotController.Config.Active.IntervalLoadGame);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _loadGameTimer?.Dispose();
        }

        public void LoadGame()
        {
            if (_gameOperation.IsTimeLimitDialogInProgress())
            {
                _gameOperation.DismissTimeLimitDialog();
            }
            if (_gameOperation.CheckSanity())
            {
                ExecuteGameLoad();
            }
        }

        private void ExecuteGameLoad()
        {
            _gameOperation.SelectNextGame(_gameOperation.GoToNextGame);
            _gameOperation.GoToNextGame = false;
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

        private bool IsRequestGameValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>() { });
            var isGeneralRule = _stateChecker.IsChooser && !_gameOperation.GameIsRunning;
            return !isBlocked && isGeneralRule;
        }

        private void RequestGame()
        {
            if (!IsRequestGameValid())
            {
                _logger.Info($"RequestGame was not valid!", GetType().Name);
                return;
            }
            _robotController.BlockOtherOperations(RobotStateAndOperations.GameOperation_LoadGame);
            _logger.Info($"LoadGame Requested Received! Sanity Counter = {_gameOperation.SanityCounter}", GetType().Name);
            _gameOperation.SanityCounter++;
            LoadGame();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _logger.Info($"GameInitializationCompletedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _gameOperation.GameIsRunning = true;
                    _gameOperation.SanityCounter = 0;
                    _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameOperation_LoadGame);
                    _gameOperation.BalanceCheckWithDelay(Constants.BalanceCheckDelayDuration);
                });


            _eventBus.Subscribe<GameLoadRequestEvent>(
                this,
                _ =>
                {
                    _logger.Info($"GameLoadRequestEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    RequestGame();
                });

            _eventBus.Subscribe<GameExitedNormalEvent>(
                 this,
                 evt =>
                 {
                     _logger.Info($"GameProcessExitedEvent-Normal Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     if (_gameOperation.IsRegularRobots())
                     {
                         _gameOperation.GameIsRunning = false;
                         _gameOperation.LoadGameWithDelay(Constants.loadGameDelayDuration);
                     }
                 });
        }
    }
}

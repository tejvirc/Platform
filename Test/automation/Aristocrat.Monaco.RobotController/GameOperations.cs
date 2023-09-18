namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Hhr.Events;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Threading.Tasks;

    internal class GameOperations : IRobotOperations
    {
        protected readonly Automation _automator;
        protected readonly IEventBus _eventBus;
        protected readonly IGameService _gameService;
        protected readonly IPropertiesManager _propertyManager;
        protected readonly RobotController _robotController;
        protected readonly RobotLogger _logger;
        protected readonly StateChecker _stateChecker;
        public bool GameIsRunning;
        public bool GoToNextGame;
        public int SanityCounter;
        private bool _disposed;

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

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Reset()
        {
            _disposed = false;
            SanityCounter = 0;
            GameIsRunning = _gameService.Running;
            GoToNextGame = false;
        }

        public virtual void Execute()
        {
            _logger.Info("GameOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
        }

        public virtual void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        public void BalanceCheckWithDelay(int milliseconds)
        {
            _logger.Info("BalanceCheckWithDelay Request Is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(_ => _eventBus.Publish(new BalanceCheckEvent()));
        }

        public bool CheckSanity()
        {
            if (SanityCounter < 5)
            {
                return true;
            }
            _logger.Error($"Game Operation Failed, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
            _robotController.Enabled = false;
            return false;
        }

        public void DismissTimeLimitDialog()
        {
            _automator.DismissTimeLimitDialog(true);
        }

        public bool IsRegularRobots()
        {
            return _robotController.InProgressRequests.Contains(RobotStateAndOperations.RegularMode);
        }

        public bool IsTimeLimitDialogInProgress()
        {
            var timeLimitDialogVisible = _propertyManager.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
            var timeLimitDialogPending = _propertyManager.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            return timeLimitDialogVisible || timeLimitDialogPending;
        }

        public void LoadGameWithDelay(int milliseconds)
        {
            _logger.Info($"LoadGameWithDelay Request is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(
                _ =>
                {
                    _eventBus.Publish(new GameLoadRequestEvent());
                });
        }

        public void SelectNextGame(bool goToNextGame)
        {
            if (!goToNextGame)
            {
                return;
            }
            _logger.Info("SelectNextGame Request Is Received!", GetType().Name);
            _robotController.Config.SelectNextGame();
        }

        private void SubscribeToEvents()
        {
            
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

            _eventBus.Subscribe<GamePlayStateChangedEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info($"GamePlayStateChangedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _robotController.IdleDuration = 0;
                     SanityCounter = 0;
                 });

            _eventBus.Subscribe<GameFatalErrorEvent>(
                 this,
                 _ =>
                 {
                     _logger.Error($"GameFatalErrorEvent Got Triggered! There is an issue with [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _robotController.Enabled = false;
                 });

            _eventBus.Subscribe<SystemEnabledEvent>(
                this,
                _ =>
                {
                    _logger.Info($"SystemEnabledEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    LoadGameWithDelay(Constants.loadGameDelayDuration);
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

        private void ToggleJackpotKey(int waitDuration)
        {
            _logger.Info($"ToggleJackpotKey Request Is Received! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
            Task.Delay(waitDuration).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
        }
    }
}
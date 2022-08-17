namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;

    internal class PlayerOperations : IRobotOperations
    {
        private readonly Dictionary<Actions, Action<Random>> _actionPlayerFunctions;
        private readonly RobotLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly StateChecker _sc;
        private readonly RobotController _robotController;
        private Timer _actionPlayerTimer;
        private bool _disposed;

        public PlayerOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _actionPlayerFunctions = new Dictionary<Actions, Action<Random>>();
            _robotController = robotController;

            InitializeActionPlayer();
        }

        ~PlayerOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            _disposed = false;
        }

        public void Execute()
        {
            _logger.Info("PlayerOperations Has Been Initiated!", GetType().Name);
            _actionPlayerTimer = new Timer(
                _ => RequestPlay(),
               null,
               _robotController.Config.Active.IntervalAction,
               _robotController.Config.Active.IntervalAction);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _actionPlayerTimer?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _actionPlayerTimer?.Dispose();
                _actionPlayerTimer = null;
                _eventBus.UnsubscribeAll(this);
                _disposed = true;
            }
        }

        private void RequestPlay()
        {
            if (!IsValid())
            {
                return;
            }
            _logger.Info("RequestPlay Received!", GetType().Name);


            var rng = new Random();
            var action = _robotController.Config.CurrentGameProfile.RobotActions.GetRandomElement(rng);
            _actionPlayerFunctions[action](rng);
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _sc.IsGame && !_sc.IsGameLoading;
        }

        private void InitializeActionPlayer()
        {
            _actionPlayerFunctions[Actions.SpinRequest] = _ =>
            {
                _logger.Info("Spin Request", GetType().Name);
                _automator.SpinRequest();
            };

            _actionPlayerFunctions[Actions.BetLevel] = (rng) =>
            {
                var betLevel = _robotController.Config.GetBetIndices().GetRandomElement(rng);
                _logger.Info($"Changing bet level: {betLevel}", GetType().Name);
                _automator.SetBetLevel(betLevel);
            };

            _actionPlayerFunctions[Actions.BetMax] = _ =>
            {
                _logger.Info("Bet Max", GetType().Name);
                _automator.SetBetMax();
            };

            _actionPlayerFunctions[Actions.LineLevel] = (rng) =>
            {
                _logger.Info("Change Line Level", GetType().Name);
                var lineLevel = _robotController.Config.GetLineIndices().GetRandomElement(rng);
                _automator.SetLineLevel(lineLevel);
            };
        }
    }
}

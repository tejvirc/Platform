namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;

    internal class PlayerOperations : IRobotOperations
    {
        private readonly Dictionary<Actions,Action<Random>> _actionPlayerFunctions;
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
                               (sender) =>
                               {
                                   RequestPlay();
                               },
                               null,
                               _robotController.Config.Active.IntervalAction,
                               _robotController.Config.Active.IntervalAction);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _actionPlayerTimer?.Dispose();
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
                if (_actionPlayerTimer is not null)
                {
                    _actionPlayerTimer.Dispose();
                }
                _actionPlayerTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestPlay()
        {
            if (!IsValid())
            {
                return;
            }
            _logger.Info("RequestPlay Received!", GetType().Name);
            var Rng = new Random((int)DateTime.Now.Ticks);
            var action =
                _robotController.Config.CurrentGameProfile.RobotActions.ElementAt(Rng.Next(_robotController.Config.CurrentGameProfile.RobotActions.Count));
            _actionPlayerFunctions[action](Rng);
        }

        private bool IsValid()
        {
            return _sc.IsGame && !_sc.IsGameLoading;
        }

        private void InitializeActionPlayer()
        {
            _actionPlayerFunctions.Add(Actions.SpinRequest,
            (Rng) =>
            {
                _logger.Info("Spin Request", GetType().Name);
                _automator.SpinRequest();
            });

            _actionPlayerFunctions.Add(Actions.BetLevel,
            (Rng) =>
            {
                _logger.Info("Changing bet level", GetType().Name);
                var betIndices = _robotController.Config.GetBetIndices();
                var index = Math.Min(betIndices[Rng.Next(betIndices.Count)], 5);
                _automator.SetBetLevel(index);
            });

            _actionPlayerFunctions.Add(Actions.BetMax,
            (Rng) =>
            {
                _logger.Info("Bet Max", GetType().Name);
                _automator.SetBetMax();
            });

            _actionPlayerFunctions.Add(Actions.LineLevel,
            (Rng) =>
            {
                _logger.Info("Change Line Level", GetType().Name);
                var lineIndices = _robotController.Config.GetLineIndices();
                _automator.SetLineLevel(lineIndices[Rng.Next(lineIndices.Count)]);
            });
        }
    }
}
